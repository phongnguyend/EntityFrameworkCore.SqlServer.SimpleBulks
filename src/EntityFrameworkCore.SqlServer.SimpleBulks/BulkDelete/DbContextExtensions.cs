using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete
{
    public static class DbContextExtensions
    {
        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idSelector);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idColumn);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idColumns);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idSelector);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, string idColumn)
        {
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idColumn);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns)
        {
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idColumns);
        }
    }
}
