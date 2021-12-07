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
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, string idColumn)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }
    }
}
