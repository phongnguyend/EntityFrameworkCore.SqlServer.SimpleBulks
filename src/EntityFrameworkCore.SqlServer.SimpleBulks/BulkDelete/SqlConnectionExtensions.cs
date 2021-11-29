using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete
{
    public static class SqlConnectionExtensions
    {
        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, SqlTransaction transaction = null)
        {
            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, string idColumn, SqlTransaction transaction = null)
        {
            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, SqlTransaction transaction = null)
        {
            new BulkDeleteBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .ToTable(tableName)
                .Execute();
        }
    }
}
