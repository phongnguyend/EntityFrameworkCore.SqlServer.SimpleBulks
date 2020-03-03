using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
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
            dbContext.GetSqlConnection().BulkInsert(data, columnNamesSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames)
        {
            dbContext.GetSqlConnection().BulkInsert(data, columnNames);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNamesSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames)
        {
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNames);
        }

        private static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            return dbContext.Database.GetDbConnection().AsSqlConnection();
        }
    }
}
