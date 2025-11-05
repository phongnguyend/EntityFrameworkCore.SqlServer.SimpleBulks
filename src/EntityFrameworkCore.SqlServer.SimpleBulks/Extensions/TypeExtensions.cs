using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class TypeExtensions
{
    private static Dictionary<Type, string> _mappings = new Dictionary<Type, string>
        {
            {typeof(bool), "bit"},
            {typeof(DateTime), "datetime2"},
            {typeof(DateTimeOffset), "datetimeoffset"},
            {typeof(decimal), "decimal(38, 20)"},
            {typeof(double), "float"},
            {typeof(Guid), "uniqueidentifier"},
            {typeof(short), "smallint"},
            {typeof(int), "int"},
            {typeof(long), "bigint"},
            {typeof(float), "real"},
            {typeof(string), "nvarchar(max)"},
        };

    public static string ToSqlType(this Type type)
    {
        var sqlType = _mappings.TryGetValue(type, out string value) ? value : "nvarchar(max)";
        return sqlType;
    }
}
