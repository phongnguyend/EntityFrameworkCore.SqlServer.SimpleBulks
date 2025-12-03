using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
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

    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties()
    {
        return PropertiesCache.GetProperties(typeof(T));
    }

    public static PropertyInfo GetProperty(string name)
    {
        if (name.Contains('.'))
        {
            return _nestedPropertiesCache.GetOrAdd(name, _ =>
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
            });
        }

        return GetProperties().TryGetValue(name, out var property) ? property : null;
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

    public static object GetPropertyValue(string name, T item)
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

    public static object GetPropertyValue(string propName, T item, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        var value = GetPropertyValue(propName, item);

        if (valueConverters != null && valueConverters.TryGetValue(propName, out var converter))
        {
            return converter.ConvertToProvider(value);
        }

        return value;
    }

    public static void SetPropertyValue(string name, T item, object value)
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

        SetPropertyValue(name, item, tempValue);
    }
}
