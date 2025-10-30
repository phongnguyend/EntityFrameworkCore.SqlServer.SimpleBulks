using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class DbContextExtensions
{
    private readonly record struct CacheKey(Type DbContextType, Type EntityType);

    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyList<ColumnInfor>> _propertiesCache = [];
    private static readonly ConcurrentDictionary<CacheKey, TableInfor> _tableInfoCache = [];
    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyDictionary<string, string>> _columnNamesCache = [];
    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyDictionary<string, string>> _columnTypesCache = [];
    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyList<string>> _primaryKeysCache = [];
    private static readonly ConcurrentDictionary<CacheKey, ColumnInfor> _outputIdCache = [];
    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyList<string>> _insertablePropertyNamesCache = [];
    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyList<string>> _allPropertyNamesCache = [];
    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyList<string>> _allPropertyNamesWithoutRowVersionsCache = [];
    private static readonly ConcurrentDictionary<CacheKey, IReadOnlyDictionary<string, ValueConverter>> _valueConvertersCache = [];

    public static TableInfor GetTableInfor(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _tableInfoCache.GetOrAdd(cacheKey, (key) =>
        {
            var entityType = dbContext.Model.FindEntityType(key.EntityType);

            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();

            var tableInfo = new DbContextTableInfor(schema, tableName, dbContext)
            {
                ColumnNameMappings = dbContext.GetColumnNames(key.EntityType),
                ColumnTypeMappings = dbContext.GetColumnTypes(key.EntityType),
                ValueConverters = dbContext.GetValueConverters(key.EntityType)
            };
            return tableInfo;
        });
    }

    public static bool IsEntityType(this DbContext dbContext, Type type)
    {
        return dbContext.Model.FindEntityType(type) != null;
    }

    public static SqlConnection GetSqlConnection(this DbContext dbContext)
    {
        return dbContext.Database.GetDbConnection() as SqlConnection;
    }

    public static SqlTransaction GetCurrentSqlTransaction(this DbContext dbContext)
    {
        var transaction = dbContext.Database.CurrentTransaction;
        return transaction == null ? null : transaction.GetDbTransaction() as SqlTransaction;
    }

    public static ConnectionContext GetConnectionContext(this DbContext dbContext)
    {
        return new ConnectionContext(dbContext.GetSqlConnection(), dbContext.GetCurrentSqlTransaction());
    }

    public static IReadOnlyList<ColumnInfor> GetProperties(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _propertiesCache.GetOrAdd(cacheKey, (key) =>
        {
            var typeProperties = key.EntityType.GetProperties().Select(x => new { x.Name, x.PropertyType });
            var entityProperties = dbContext.Model.FindEntityType(key.EntityType)
                           .GetProperties();

            var data = typeProperties.Join(entityProperties,
                prop => prop.Name,
                entityProp => entityProp.Name,
                (prop, entityProp) => new ColumnInfor
                {
                    PropertyName = prop.Name,
                    PropertyType = prop.PropertyType,
                    ColumnName = entityProp.GetColumnName(),
                    ColumnType = entityProp.GetColumnType(),
                    ValueGenerated = entityProp.ValueGenerated,
                    DefaultValueSql = entityProp.GetDefaultValueSql(),
                    IsPrimaryKey = entityProp.IsPrimaryKey(),
                    IsRowVersion = entityProp.IsRowVersion(),
                    ValueConverter = entityProp.GetValueConverter(),
                }).ToArray();
            return data;
        });
    }

    public static IReadOnlyDictionary<string, string> GetColumnNames(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _columnNamesCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);
        });
    }

    public static IReadOnlyDictionary<string, string> GetColumnTypes(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _columnTypesCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.ToDictionary(x => x.PropertyName, x => x.ColumnType);
        });
    }

    public static IReadOnlyList<string> GetPrimaryKeys(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _primaryKeysCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.Where(x => x.IsPrimaryKey).Select(x => x.PropertyName).ToArray();
        });
    }

    public static ColumnInfor GetOutputId(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _outputIdCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd).FirstOrDefault();
        });
    }

    public static IReadOnlyList<string> GetInsertablePropertyNames(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _insertablePropertyNamesCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.Where(x => x.ValueGenerated == ValueGenerated.Never).Select(x => x.PropertyName).ToArray();
        });
    }

    public static IReadOnlyList<string> GetAllPropertyNames(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _allPropertyNamesCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.Select(x => x.PropertyName).ToArray();
        });
    }

    public static IReadOnlyList<string> GetAllPropertyNamesWithoutRowVersions(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _allPropertyNamesWithoutRowVersionsCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.Where(x => !x.IsRowVersion).Select(x => x.PropertyName).ToArray();
        });
    }

    public static IReadOnlyDictionary<string, ValueConverter> GetValueConverters(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);
        return _valueConvertersCache.GetOrAdd(cacheKey, (key) =>
        {
            var properties = dbContext.GetProperties(key.EntityType);
            return properties.Where(x => x.ValueConverter != null).ToDictionary(x => x.PropertyName, x => x.ValueConverter);
        });
    }

    public static IDbCommand CreateTextCommand(this DbContext dbContext, string commandText, BulkOptions options = null)
    {
        return dbContext.GetConnectionContext().CreateTextCommand(commandText, options);
    }

    public static void ExecuteReader(this DbContext dbContext, string commandText, Action<IDataReader> action, BulkOptions options = null)
    {
        using var updateCommand = dbContext.CreateTextCommand(commandText, options);
        using var reader = updateCommand.ExecuteReader();

        while (reader.Read())
        {
            action(reader);
        }
    }

    public static List<Guid> GenerateDbSequentialIds(this DbContext dbContext, int count)
    {
        var queryBuilder = new StringBuilder();

        queryBuilder.AppendLine($"create table #temp(Id uniqueidentifier default NEWSEQUENTIALID(), Value int)");
        queryBuilder.AppendLine($"insert into #temp(Value) values");
        queryBuilder.AppendLine(string.Join(',', Enumerable.Range(0, count).Select(x => $"(0)")));
        queryBuilder.AppendLine($"select Id from #temp order by Id");
        queryBuilder.AppendLine($"drop table #temp;");

        var query = queryBuilder.ToString();

        return dbContext.Database.SqlQueryRaw<Guid>(query).ToList();
    }

    public static void GenerateDbSequentialIds(this DbContext dbContext, Queue<Guid> guids, int count)
    {
        foreach (var item in GenerateDbSequentialIds(dbContext, count))
        {
            guids.Enqueue(item);
        }
    }

    public static bool IsRowVersion(this IProperty property)
    {
        return property.IsConcurrencyToken
            && property.ValueGenerated == ValueGenerated.OnAddOrUpdate
            && property.ClrType == typeof(byte[])
            && (string.Equals(property.GetColumnType(), "rowversion", StringComparison.OrdinalIgnoreCase)
                || string.Equals(property.GetColumnType(), "timestamp", StringComparison.OrdinalIgnoreCase));
    }
}
