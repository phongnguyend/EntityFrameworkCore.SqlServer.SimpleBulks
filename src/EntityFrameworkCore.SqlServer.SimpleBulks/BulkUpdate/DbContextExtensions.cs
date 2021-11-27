using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate
{
    public static class DbContextExtensions
    {
        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idSelector, columnNamesSelector);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idColumn, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idColumns, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idSelector, columnNamesSelector);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> columnNames)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idColumn, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idColumns, columnNames);
        }
    }
}
