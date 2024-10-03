using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class DbContextExtensions
{
    public static TableInfor GetTableInfor(this DbContext dbContext, Type type)
    {
        var entityType = dbContext.Model.FindEntityType(type);

        var schema = entityType.GetSchema();
        var tableName = entityType.GetTableName();

        return new TableInfor(schema, tableName);
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

    public static IList<ColumnInfor> GetProperties(this DbContext dbContext, Type type)
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
                IsPrimaryKey = entityProp.IsPrimaryKey()
            });

        return data.ToList();
    }

    public static Dictionary<string, string> GetMappedColumns(this DbContext dbContext, Type type)
    {
        var typeProperties = type.GetProperties().Select(x => new { x.Name, x.PropertyType });
        var entityProperties = dbContext.Model.FindEntityType(type)
                       .GetProperties().Select(x => new { x.Name, ColumnName = x.GetColumnName(), ColumnType = x.GetColumnType() });

        var data = typeProperties.Join(entityProperties, prop => prop.Name, entityProp => entityProp.Name, (prop, entityProp) => new { prop.Name, prop.PropertyType, entityProp.ColumnName, entityProp.ColumnType });

        return data.ToDictionary(x => x.Name, x => x.ColumnName);
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
}
