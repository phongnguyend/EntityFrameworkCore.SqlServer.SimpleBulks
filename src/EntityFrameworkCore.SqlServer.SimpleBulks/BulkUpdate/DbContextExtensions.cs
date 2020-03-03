using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
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
            dbContext.GetSqlConnection().BulkUpdate(data, idSelector, columnNamesSelector);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, idColumn, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            dbContext.GetSqlConnection().BulkUpdate(data, idColumns, columnNames);
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

        private static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            return dbContext.Database.GetDbConnection().AsSqlConnection();
        }
    }
}
