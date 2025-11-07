using System;
using System.Collections.Concurrent;
using System.Data;
using System.Text.RegularExpressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, string> _mappings = new ConcurrentDictionary<Type, string>();

    private static readonly ConcurrentDictionary<string, SqlDbType> _sqlTypeCache = new();

    static TypeExtensions()
    {
        ConfigureSqlServerTypeMapping<bool>("bit");
        ConfigureSqlServerTypeMapping<DateTime>("datetime2");
        ConfigureSqlServerTypeMapping<DateTimeOffset>("datetimeoffset");
        ConfigureSqlServerTypeMapping<decimal>("decimal(38, 20)");
        ConfigureSqlServerTypeMapping<double>("float");
        ConfigureSqlServerTypeMapping<Guid>("uniqueidentifier");
        ConfigureSqlServerTypeMapping<short>("smallint");
        ConfigureSqlServerTypeMapping<int>("int");
        ConfigureSqlServerTypeMapping<long>("bigint");
        ConfigureSqlServerTypeMapping<float>("real");
        ConfigureSqlServerTypeMapping<string>("nvarchar(max)");
    }

    public static void ConfigureSqlServerTypeMapping<T>(string sqlServerType)
    {
        ConfigureSqlServerTypeMapping(typeof(T), sqlServerType);
    }

    public static void ConfigureSqlServerTypeMapping(Type type, string sqlServerType)
    {
        _mappings[type] = sqlServerType;
    }

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
