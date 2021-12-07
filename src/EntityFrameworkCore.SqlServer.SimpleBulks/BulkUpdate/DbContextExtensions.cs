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
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> columnNames)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }
    }
}
