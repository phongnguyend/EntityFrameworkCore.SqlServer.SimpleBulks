using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
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
            var sqlType = _mappings.ContainsKey(type) ? _mappings[type] : "nvarchar(max)";
            return sqlType;
        }

        public static string[] GetDbColumnNames(this Type type, params string[] ignoredColumns)
        {
            var names = type.GetProperties()
                .Where(x => IsSupportedType(x))
                .Where(x => ignoredColumns == null || !ignoredColumns.Contains(x.Name))
                .Select(x => x.Name);
            return names.ToArray();
        }

        public static string[] GetUnSupportedPropertyNames(this Type type)
        {
            var names = type.GetProperties()
                .Where(x => !IsSupportedType(x))
                .Select(x => x.Name);
            return names.ToArray();
        }

        private static bool IsSupportedType(PropertyInfo property)
        {
            return _mappings.Keys.Contains(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType) || property.PropertyType.IsValueType;
        }
    }
}
