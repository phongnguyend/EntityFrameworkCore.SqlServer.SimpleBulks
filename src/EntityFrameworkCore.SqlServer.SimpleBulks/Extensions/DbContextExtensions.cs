using Microsoft.Data.SqlClient;
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
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNamesSelector);
        }

        public static void BulkInsert<T>(this DbContext dbContext, IList<T> data, string tableName, params string[] columnNames)
        {
            dbContext.GetSqlConnection().BulkInsert(data, tableName, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idSelector, columnNamesSelector);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> data, string tableName, string idColumn, params string[] columnNames)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idColumn, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> data, string tableName, List<string> idColumns, params string[] columnNames)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, tableName, idColumns, columnNames);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idSelector);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IList<T> data, string tableName, string idColumn)
        {
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idColumn);
        }

        public static void BulkDelete<T>(this DbContext dbContext, IList<T> data, string tableName, List<string> idColumns)
        {
            dbContext.GetSqlConnection().BulkDelete(data, tableName, idColumns);
        }

        private static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            return dbContext.Database.GetDbConnection().AsSqlConnection();
        }
    }
}
