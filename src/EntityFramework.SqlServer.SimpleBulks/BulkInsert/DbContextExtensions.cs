using EntityFramework.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.SqlServer.SimpleBulks.BulkInsert
{
    public static class DbContextExtensions
    {
        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var columns = properties
                .Where(x => x.ValueGenerated == StoreGeneratedPattern.None)
                .Select(x => x.PropertyName);
            var idColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == StoreGeneratedPattern.Identity)
                .Select(x => x.PropertyName)
                .FirstOrDefault();
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columns)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .WithOuputId(idColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var idColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == StoreGeneratedPattern.Identity)
                .Select(x => x.PropertyName)
                .FirstOrDefault();
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .WithOuputId(idColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
