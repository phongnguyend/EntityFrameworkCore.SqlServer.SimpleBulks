using System;
using System.Collections.Concurrent;
using System.Data;
using System.Text.RegularExpressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public static class TypeMapper
{
    private static readonly ConcurrentDictionary<Type, string> _mappings = new ConcurrentDictionary<Type, string>();

    private static readonly ConcurrentDictionary<string, SqlDbType> _sqlTypeCache = new();

    static TypeMapper()
    {
        ConfigureSqlServerType<bool>("bit");
        ConfigureSqlServerType<DateTime>("datetime2");
        ConfigureSqlServerType<DateTimeOffset>("datetimeoffset");
        ConfigureSqlServerType<decimal>("decimal(38, 20)");
        ConfigureSqlServerType<double>("float");
        ConfigureSqlServerType<Guid>("uniqueidentifier");
        ConfigureSqlServerType<short>("smallint");
        ConfigureSqlServerType<int>("int");
        ConfigureSqlServerType<long>("bigint");
        ConfigureSqlServerType<float>("real");
        ConfigureSqlServerType<string>("nvarchar(max)");
    }

    public static void ConfigureSqlServerType<T>(string sqlServerType)
    {
        ConfigureSqlServerType(typeof(T), sqlServerType);
    }

    public static void ConfigureSqlServerType(Type type, string sqlServerType)
    {
        _mappings[type] = sqlServerType;
    }

    public static string ToSqlDbType(this Type type)
    {
        if (type.IsEnum)
        {
            return "int";
        }

        var sqlType = _mappings.TryGetValue(type, out var value) ? value : "nvarchar(max)";
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
