using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate
{
    public static class DbContextExtensions
    {
        public static BulkUpdateResult DirectUpdate<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var primaryKeys = properties
                .Where(x => x.IsPrimaryKey)
                .Select(x => x.PropertyName);
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkUpdateBuilder<T>(connection, transaction)
                 .WithId(primaryKeys)
                 .WithColumns(columnNamesSelector)
                 .WithDbColumnMappings(dbColumnMappings)
                 .ToTable(tableName)
                 .ConfigureBulkOptions(configureOptions)
                 .SingleUpdate(data);
        }

        public static BulkUpdateResult DirectUpdate<T>(this DbContext dbContext, T data, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var primaryKeys = properties
                .Where(x => x.IsPrimaryKey)
                .Select(x => x.PropertyName);
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkUpdateBuilder<T>(connection, transaction)
                .WithId(primaryKeys)
                .WithColumns(columnNames)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .SingleUpdate(data);
        }
    }
}
