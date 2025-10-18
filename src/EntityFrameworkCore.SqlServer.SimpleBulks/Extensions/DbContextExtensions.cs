﻿using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class DbContextExtensions
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<ColumnInfor>> _propertiesCache = [];
    private static readonly ConcurrentDictionary<Type, TableInfor> _tableInfoCache = [];

    public static TableInfor GetTableInfor(this DbContext dbContext, Type type)
    {
        return _tableInfoCache.GetOrAdd(type, (type) =>
        {
            var entityType = dbContext.Model.FindEntityType(type);

            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();

            var tableInfo = new TableInfor(schema, tableName);
            return tableInfo;
        });
    }

    public static bool IsEntityType(this DbContext dbContext, Type type)
    {
        return dbContext.Model.FindEntityType(type) != null;
    }

    public static SqlConnection GetSqlConnection(this DbContext dbContext)
    {
        return dbContext.Database.GetDbConnection().AsSqlConnection();
    }

    public static SqlTransaction GetCurrentSqlTransaction(this DbContext dbContext)
    {
        var transaction = dbContext.Database.CurrentTransaction;
        return transaction == null ? null : transaction.GetDbTransaction() as SqlTransaction;
    }

    public static IReadOnlyList<ColumnInfor> GetProperties(this DbContext dbContext, Type type)
    {
        return _propertiesCache.GetOrAdd(type, (type) =>
        {
            var typeProperties = type.GetProperties().Select(x => new { x.Name, x.PropertyType });
            var entityProperties = dbContext.Model.FindEntityType(type)
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
                    IsRowVersion = entityProp.IsRowVersion()
                }).ToList();
            return data;
        });
    }

    public static IDbCommand CreateTextCommand(this DbContext dbContext, string commandText, BulkOptions options = null)
    {
        return dbContext.GetSqlConnection().CreateTextCommand(dbContext.GetCurrentSqlTransaction(), commandText, options);
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
