using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert
{
    public static class DbContextExtensions
    {
        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var columns = properties
                .Where(x => x.ValueGenerated == ValueGenerated.Never)
                .Select(x => x.PropertyName);
            var idColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
                .Select(x => x.PropertyName)
                .FirstOrDefault();
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columns)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .WithOutputId(idColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var idColumn = properties
                .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
                .Select(x => x.PropertyName)
                .FirstOrDefault();
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .WithDbColumnMappings(dbColumnMappings)
                .ToTable(tableName)
                .WithOutputId(idColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
