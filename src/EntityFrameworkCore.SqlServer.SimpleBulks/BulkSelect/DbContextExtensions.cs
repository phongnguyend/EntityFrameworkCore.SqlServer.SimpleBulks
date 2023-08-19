using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkSelect
{
    public static class DbContextExtensions
    {
        public static IEnumerable<T> BulkSelect<T>(this DbContext dbContext, Expression<Func<T, object>> matchColumnsSelector, IEnumerable<T> machedValues, Action<BulkSelectOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var columns = properties.Select(x => x.PropertyName);
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkSelectBuilder<T>(connection, transaction)
                 .WithColumns(columns)
                 .WithDbColumnMappings(dbColumnMappings)
                 .FromTable(tableName)
                 .WithMatchedColumns(matchColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static IEnumerable<T> BulkSelect<T>(this DbContext dbContext, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> matchColumnsSelector, IEnumerable<T> machedValues, Action<BulkSelectOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkSelectBuilder<T>(connection, transaction)
                 .WithColumns(columnNamesSelector)
                 .WithDbColumnMappings(dbColumnMappings)
                 .FromTable(tableName)
                 .WithMatchedColumns(matchColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }
    }
}
