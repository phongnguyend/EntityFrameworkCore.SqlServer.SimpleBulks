using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlConverters
{
    public static class SqlTypeConverter
    {
        private static Dictionary<Type, string> _mappings = new Dictionary<Type, string>();

        static SqlTypeConverter()
        {
            _mappings = new Dictionary<Type, string>
            {
                {typeof(bool), "bit"},
                {typeof(DateTime), "datetime"},
                {typeof(decimal), "decimal(38, 20)"},
                {typeof(double), "double"},
                {typeof(Guid), "uniqueidentifier"},
                {typeof(short), "smallint"},
                {typeof(int), "int"},
                {typeof(long), "bigint"},
                {typeof(float), "single"},
                {typeof(string), "nvarchar(max)"},
            };
        }

        public static string Convert(Type type)
        {
            return _mappings.ContainsKey(type) ? _mappings[type] : "nvarchar(max)";
        }
    }
}
