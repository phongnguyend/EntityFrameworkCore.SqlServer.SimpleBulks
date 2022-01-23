using EntityFramework.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.SqlServer.SimpleBulks.BulkMerge
{
    public static class DbContextExtensions
    {
        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithUpdateColumns(updateColumnNamesSelector)
                .WithInsertColumns(insertColumnNamesSelector)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
