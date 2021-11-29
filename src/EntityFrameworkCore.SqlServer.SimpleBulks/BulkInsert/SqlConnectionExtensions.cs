using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert
{
    public static class SqlConnectionExtensions
    {
        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, SqlTransaction transaction = null)
        {
            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, SqlTransaction transaction = null)
        {
            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, SqlTransaction transaction = null)
        {
            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, string idColumnName, SqlTransaction transaction = null)
        {
            new BulkInsertBuilder<T>(connection, transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .Execute();
        }
    }
}
