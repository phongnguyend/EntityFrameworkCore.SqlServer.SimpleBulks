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
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumn)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumn)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .Execute();
        }

        public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, string idColumn)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumn)
                .Execute();
        }
    }
}
