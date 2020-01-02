using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class DbContextExtensions
    {
        public static void BulkInsert<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            dbContext.Database.GetDbConnection().BulkInsert(data, tableName, columnNamesSelector);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            dbContext.Database.GetDbConnection().BulkUpdate(data, tableName, idSelector, columnNamesSelector);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            dbContext.Database.GetDbConnection().BulkDelete(data, tableName, idSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IList<T> data, string tableName, params string[] columnNames)
        {
            dbContext.Database.GetDbConnection().BulkInsert(data, tableName, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> data, string tableName, string idColumn, params string[] columnNames)
        {
            dbContext.Database.GetDbConnection().BulkUpdate(data, tableName, idColumn, columnNames);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IList<T> data, string tableName, string idColumn)
        {
            dbContext.Database.GetDbConnection().BulkDelete(data, tableName, idColumn);
        }
    }
}
