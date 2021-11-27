using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert
{
    public static class DbContextExtensions
    {
        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNamesSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNames);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNamesSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames)
        {
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNames);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNamesSelector, idSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumn)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNames, idColumn);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNamesSelector, idSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, string idColumn)
        {
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNames, idColumn);
        }
    }
}
