using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class PropertiesCache
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> _cache = new();

    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties(Type type)
    {
        return _cache.GetOrAdd(type, _ =>
        {
            var properties = type.GetProperties();
            return properties.ToFrozenDictionary(p => p.Name, p => p);
        });
    }

    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties<T>()
    {
        return GetProperties(typeof(T));
    }
}

public class PropertiesCache<T>
{
    private static readonly ConcurrentDictionary<string, PropertyInfo> _nestedPropertiesCache = new();
    private static readonly ConcurrentDictionary<string, Func<T, object>> _nestedPropertyGetterCache = new();
    private static readonly ConcurrentDictionary<string, Action<T, object>> _nestedPropertySetterCache = new();

    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties()
    {
        return PropertiesCache.GetProperties(typeof(T));
    }

    public static PropertyInfo GetProperty(string name)
    {
        return _nestedPropertiesCache.GetOrAdd(name, _ =>
        {
            if (name.Contains('.'))
            {
                var propertyNames = name.Split('.');
                var currentType = typeof(T);
                PropertyInfo currentProperty = null;

                foreach (var propertyName in propertyNames)
                {
                    var properties = PropertiesCache.GetProperties(currentType);

                    if (!properties.TryGetValue(propertyName, out currentProperty))
                    {
                        return null; // Property not found in the chain
                    }

                    currentType = currentProperty.PropertyType;
                }

                return currentProperty;
            }

            return GetProperties().TryGetValue(name, out var property) ? property : null;
        });
    }

    public static Type GetPropertyUnderlyingType(string name)
    {
        var property = GetProperty(name);

        return Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
    }

    public static Type GetPropertyUnderlyingType(string name, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        if (valueConverters != null && valueConverters.TryGetValue(name, out var converter))
        {
            return Nullable.GetUnderlyingType(converter.ProviderClrType) ?? converter.ProviderClrType;
        }

        return GetPropertyUnderlyingType(name);
    }

    public static object GetPropertyValueReflection(string name, T item)
    {
        if (name.Contains('.'))
        {
            var propertyNames = name.Split('.');
            object currentObject = item;
            PropertyInfo currentProperty = null;

            foreach (var propertyName in propertyNames)
            {
                var properties = PropertiesCache.GetProperties(currentObject.GetType());
                if (!properties.TryGetValue(propertyName, out currentProperty))
                {
                    throw new ArgumentException($"Property '{name}' not found.");
                }
                currentObject = currentProperty.GetValue(currentObject);
                if (currentObject == null)
                {
                    return null; // Return null if any intermediate property is null
                }
            }

            return currentObject;
        }

        var prop = GetProperty(name);
        return prop.GetValue(item);
    }

    public static object GetPropertyValueOptimized(string name, T item)
    {
        var getter = _nestedPropertyGetterCache.GetOrAdd(name, propName =>
        {
            if (name.Contains('.'))
            {
                return BuildNestedPropertyAccessorWithNullChecks(name);
            }

            var prop = GetProperty(propName);
            var parameter = Expression.Parameter(typeof(T), "obj");
            var propertyAccess = Expression.Property(parameter, prop);
            var convertToObject = Expression.Convert(propertyAccess, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(convertToObject, parameter);

            return lambda.Compile();
        });

        return getter(item);
    }

    private static Func<T, object> BuildNestedPropertyAccessorWithNullChecks(string fullPropertyPath)
    {
        var propertyNames = fullPropertyPath.Split('.');

        var parameter = Expression.Parameter(typeof(T), "obj");

        Expression current = parameter;
        var nullCheckExpressions = new List<Expression>();

        // Build property chain and collect null checks
        foreach (var propertyName in propertyNames)
        {
            var propertyInfo = current.Type.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{fullPropertyPath}' not found.");
            }

            // Add null check for reference types before accessing the property
            if (!current.Type.IsValueType)
            {
                nullCheckExpressions.Add(
                    Expression.Equal(current, Expression.Constant(null, current.Type))
                );
            }

            current = Expression.Property(current, propertyInfo);
        }

        // Convert final result to object
        var convertToObject = Expression.Convert(current, typeof(object));

        // Build the conditional expression: if any null checks are true, return null, else return value
        Expression body;
        if (nullCheckExpressions.Count > 0)
        {
            // Combine all null checks with OR: check1 || check2 || check3
            Expression combinedNullCheck = nullCheckExpressions[0];
            for (int i = 1; i < nullCheckExpressions.Count; i++)
            {
                combinedNullCheck = Expression.OrElse(combinedNullCheck, nullCheckExpressions[i]);
            }

            // Build: combinedNullCheck ? null : (object)value
            body = Expression.Condition(
                combinedNullCheck,
                Expression.Constant(null, typeof(object)),
                convertToObject
            );
        }
        else
        {
            // No null checks needed (all value types in chain)
            body = convertToObject;
        }

        var lambda = Expression.Lambda<Func<T, object>>(body, parameter);
        return lambda.Compile();
    }

    private static Action<T, object> BuildNestedPropertySetterWithObjectCreation(string fullPropertyPath)
    {
        var propertyNames = fullPropertyPath.Split('.');

        var parameter = Expression.Parameter(typeof(T), "obj");
        var valueParameter = Expression.Parameter(typeof(object), "val");

        var statements = new List<Expression>();
        var variables = new List<ParameterExpression>();

        Expression current = parameter;
        var currentVarIndex = 0;

        // Navigate through the property chain and create intermediate objects if needed
        for (int i = 0; i < propertyNames.Length; i++)
        {
            var propertyName = propertyNames[i];
            var propertyInfo = current.Type.GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new ArgumentException($"Property '{fullPropertyPath}' not found.");
            }

            if (i < propertyNames.Length - 1)
            {
                // Intermediate property - need to handle null and create object if needed
                var propertyAccess = Expression.Property(current, propertyInfo);
                var tempVar = Expression.Variable(propertyInfo.PropertyType, $"temp{currentVarIndex++}");
                variables.Add(tempVar);

                // Assign current property value to temp variable
                statements.Add(Expression.Assign(tempVar, propertyAccess));

                // Check if null and create new instance
                if (!propertyInfo.PropertyType.IsValueType)
                {
                    var nullCheck = Expression.Equal(tempVar, Expression.Constant(null, propertyInfo.PropertyType));
                    var createNew = Expression.Assign(tempVar, Expression.New(propertyInfo.PropertyType));
                    var setProperty = Expression.Assign(propertyAccess, tempVar);

                    var ifNull = Expression.IfThen(
                        nullCheck,
                        Expression.Block(createNew, setProperty)
                    );

                    statements.Add(ifNull);
                }

                current = tempVar;
            }
            else
            {
                // Final property - perform the assignment
                var propertyAccess = Expression.Property(current, propertyInfo);
                var convertedValue = Expression.Convert(valueParameter, propertyInfo.PropertyType);
                statements.Add(Expression.Assign(propertyAccess, convertedValue));
            }
        }

        var body = Expression.Block(variables, statements);
        var lambda = Expression.Lambda<Action<T, object>>(body, parameter, valueParameter);

        return lambda.Compile();
    }

    public static object GetFlattenedPropertyValueReflection(string name, T item)
    {
        var prop = GetProperty(name);
        return prop.GetValue(item);
    }

    public static object GetFlattenedPropertyValueOptimized(string name, T item)
    {
        var getter = _nestedPropertyGetterCache.GetOrAdd(name, propName =>
        {
            var prop = GetProperty(propName);
            var parameter = Expression.Parameter(typeof(T), "obj");
            var propertyAccess = Expression.Property(parameter, prop);
            var convertToObject = Expression.Convert(propertyAccess, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(convertToObject, parameter);

            return lambda.Compile();
        });

        return getter(item);
    }

    public static object GetPropertyValue(string propName, T item, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        var value = GetPropertyValueOptimized(propName, item);

        if (valueConverters != null && valueConverters.TryGetValue(propName, out var converter))
        {
            return converter.ConvertToProvider(value);
        }

        return value;
    }

    public static void SetPropertyValueReflection(string name, T item, object value)
    {
        if (name.Contains('.'))
        {
            var propertyNames = name.Split('.');
            object currentObject = item;
            PropertyInfo currentProperty = null;

            int i = 1;
            int count = propertyNames.Length;

            foreach (var propertyName in propertyNames)
            {
                var properties = PropertiesCache.GetProperties(currentObject.GetType());
                if (!properties.TryGetValue(propertyName, out currentProperty))
                {
                    throw new ArgumentException($"Property '{name}' not found.");
                }

                if (i == count)
                {
                    break;
                }

                var currentPropertyValue = currentProperty.GetValue(currentObject);
                if (currentPropertyValue == null)
                {
                    currentPropertyValue = Activator.CreateInstance(currentProperty.PropertyType);
                    currentProperty.SetValue(currentObject, currentPropertyValue, null);
                }

                currentObject = currentPropertyValue;

                i++;
            }

            currentProperty.SetValue(currentObject, value, null);
            return;
        }

        var prop = GetProperty(name);
        prop.SetValue(item, value, null);
    }

    public static void SetPropertyValueOptimized(string name, T item, object value)
    {
        var setter = _nestedPropertySetterCache.GetOrAdd(name, propName =>
        {
            if (name.Contains('.'))
            {
                return BuildNestedPropertySetterWithObjectCreation(propName);
            }

            var prop = GetProperty(propName);
            if (prop == null)
            {
                throw new ArgumentException($"Property '{propName}' not found.");
            }

            // Create compiled expression: (T obj, object val) => obj.PropertyName = (PropertyType)val
            var parameter = Expression.Parameter(typeof(T), "obj");
            var valueParameter = Expression.Parameter(typeof(object), "val");

            var propertyAccess = Expression.Property(parameter, prop);
            var convertedValue = Expression.Convert(valueParameter, prop.PropertyType);
            var assignExpression = Expression.Assign(propertyAccess, convertedValue);

            var lambda = Expression.Lambda<Action<T, object>>(assignExpression, parameter, valueParameter);
            return lambda.Compile();
        });

        setter(item, value);
    }

    public static void SetFlattenedValueReflection(string name, T item, object value)
    {
        var prop = GetProperty(name);
        prop.SetValue(item, value, null);
    }

    public static void SetFlattenedValueOptimized(string name, T item, object value)
    {
        var setter = _nestedPropertySetterCache.GetOrAdd(name, propName =>
        {
            var prop = GetProperty(propName);
            if (prop == null)
            {
                throw new ArgumentException($"Property '{propName}' not found.");
            }

            // Create compiled expression: (T obj, object val) => obj.PropertyName = (PropertyType)val
            var parameter = Expression.Parameter(typeof(T), "obj");
            var valueParameter = Expression.Parameter(typeof(object), "val");

            var propertyAccess = Expression.Property(parameter, prop);
            var convertedValue = Expression.Convert(valueParameter, prop.PropertyType);
            var assignExpression = Expression.Assign(propertyAccess, convertedValue);

            var lambda = Expression.Lambda<Action<T, object>>(assignExpression, parameter, valueParameter);
            return lambda.Compile();
        });

        setter(item, value);
    }

    public static void SetPropertyValue(string name, T item, object value, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        object tempValue = value;

        if (valueConverters != null && valueConverters.TryGetValue(name, out var converter))
        {
            tempValue = converter.ConvertFromProvider(value);
        }
        else
        {
            var type = GetPropertyUnderlyingType(name);
            tempValue = type.IsEnum && tempValue != null ? Enum.Parse(type, tempValue.ToString()) : tempValue;
        }

        SetPropertyValueOptimized(name, item, tempValue);
    }
}
