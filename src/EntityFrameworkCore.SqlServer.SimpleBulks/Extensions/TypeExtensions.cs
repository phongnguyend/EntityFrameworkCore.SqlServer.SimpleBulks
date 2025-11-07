using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

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

    private static readonly ConcurrentDictionary<string, SqlDbType> _sqlTypeCache = new();

    public static string ToSqlDbType(this Type type)
    {
        if (type.IsEnum)
        {
            return "int";
        }

        var sqlType = _mappings.TryGetValue(type, out string value) ? value : "nvarchar(max)";
        return sqlType;
    }

    public static SqlDbType ToSqlDbType(this string sqlTypeText)
    {
        if (string.IsNullOrWhiteSpace(sqlTypeText))
        {
            return SqlDbType.NVarChar;
        }

        return _sqlTypeCache.GetOrAdd(sqlTypeText, static sqlType =>
        {
            // Extract the base type name by removing scale/precision parameters
            var baseType = Regex.Replace(sqlType.ToLowerInvariant(), @"\([^)]*\)", "").Trim();

            return baseType switch
            {
                "bigint" => SqlDbType.BigInt,
                "bit" => SqlDbType.Bit,
                "char" => SqlDbType.Char,
                "date" => SqlDbType.Date,
                "datetime" => SqlDbType.DateTime,
                "datetime2" => SqlDbType.DateTime2,
                "datetimeoffset" => SqlDbType.DateTimeOffset,
                "decimal" => SqlDbType.Decimal,
                "float" => SqlDbType.Float,
                "int" => SqlDbType.Int,
                "money" => SqlDbType.Money,
                "nchar" => SqlDbType.NChar,
                "ntext" => SqlDbType.NText,
                "numeric" => SqlDbType.Decimal,
                "nvarchar" => SqlDbType.NVarChar,
                "real" => SqlDbType.Real,
                "smalldatetime" => SqlDbType.SmallDateTime,
                "smallint" => SqlDbType.SmallInt,
                "smallmoney" => SqlDbType.SmallMoney,
                "text" => SqlDbType.Text,
                "time" => SqlDbType.Time,
                "tinyint" => SqlDbType.TinyInt,
                "uniqueidentifier" => SqlDbType.UniqueIdentifier,
                "varbinary" => SqlDbType.VarBinary,
                "varchar" => SqlDbType.VarChar,
                _ => SqlDbType.NVarChar,
            };
        });
    }
}
