using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
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
            dbContext.GetSqlConnection().BulkDelete(data, idSelector);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn)
        {
            dbContext.GetSqlConnection().BulkDelete(data, idColumn);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns)
        {
            dbContext.GetSqlConnection().BulkDelete(data, idColumns);
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

        private static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            return dbContext.Database.GetDbConnection().AsSqlConnection();
        }
    }
}
