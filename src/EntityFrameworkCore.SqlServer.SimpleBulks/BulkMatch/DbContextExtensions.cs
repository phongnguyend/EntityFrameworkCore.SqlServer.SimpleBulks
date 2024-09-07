using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch
{
    public static class DbContextExtensions
    {
        public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var columns = properties.Select(x => x.PropertyName);
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkMatchBuilder<T>(connection, transaction)
                 .WithReturnedColumns(columns)
                 .WithDbColumnMappings(dbColumnMappings)
                 .WithTable(tableName)
                 .WithMatchedColumns(matchedColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkMatchBuilder<T>(connection, transaction)
                 .WithReturnedColumns(returnedColumnsSelector)
                 .WithDbColumnMappings(dbColumnMappings)
                 .WithTable(tableName)
                 .WithMatchedColumns(matchedColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }
    }
}
