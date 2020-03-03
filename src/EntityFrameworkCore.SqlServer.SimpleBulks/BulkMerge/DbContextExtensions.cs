using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge
{
    public static class DbContextExtensions
    {
        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector)
        {
            dbContext.GetSqlConnection().BulkMerge(data, idSelector, updateColumnNamesSelector, insertColumnNamesSelector);
        }

        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            dbContext.GetSqlConnection().BulkMerge(data, idColumn, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            dbContext.GetSqlConnection().BulkMerge(data, idColumns, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector)
        {
            dbContext.GetSqlConnection().BulkMerge(data, tableName, idSelector, updateColumnNamesSelector, insertColumnNamesSelector);
        }

        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            dbContext.GetSqlConnection().BulkMerge(data, tableName, idColumn, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            dbContext.GetSqlConnection().BulkMerge(data, tableName, idColumns, updateColumnNames, insertColumnNames);
        }

        private static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            return dbContext.Database.GetDbConnection().AsSqlConnection();
        }
    }
}
