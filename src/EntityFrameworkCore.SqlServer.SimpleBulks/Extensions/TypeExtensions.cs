using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class TypeExtensions
{
    private static Dictionary<Type, string> _mappings = new Dictionary<Type, string>
        {
            {typeof(bool), "bit"},
            {typeof(DateTime), "datetime2"},
            {typeof(DateTimeOffset), "datetimeoffset"},
            {typeof(decimal), "decimal(38, 20)"},
            {typeof(double), "double"},
            {typeof(Guid), "uniqueidentifier"},
            {typeof(short), "smallint"},
            {typeof(int), "int"},
            {typeof(long), "bigint"},
            {typeof(float), "single"},
            {typeof(string), "nvarchar(max)"},
        };

    public static string ToSqlType(this Type type)
    {
        var sqlType = _mappings.TryGetValue(type, out string value) ? value : "nvarchar(max)";
        return sqlType;
    }

    public static Dictionary<string, Type> GetProviderClrTypes(this Type type, IEnumerable<string> propertyNames, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        var properties = TypeDescriptor.GetProperties(type);

        var updatablePros = new List<PropertyDescriptor>();
        foreach (PropertyDescriptor prop in properties)
        {
            if (propertyNames.Contains(prop.Name))
            {
                updatablePros.Add(prop);
            }
        }

        return updatablePros.ToDictionary(x => x.Name, x => GetProviderClrType(x, valueConverters));
    }

    private static Type GetProviderClrType(PropertyDescriptor property, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        if (valueConverters != null && valueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ProviderClrType;
        }

        return Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
    }
}
