using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert
{
    public static class SqlConnectionExtensions
    {
        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, string idColumnName)
        {
            new BulkInsertBuilder<T>(connection)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .Execute();
        }
    }
}
