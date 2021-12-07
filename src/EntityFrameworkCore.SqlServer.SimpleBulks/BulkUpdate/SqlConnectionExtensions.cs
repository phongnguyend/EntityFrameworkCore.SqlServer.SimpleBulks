using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate
{
    public static class SqlConnectionExtensions
    {
        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, SqlTransaction transaction = null)
        {
            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> columnNames, SqlTransaction transaction = null)
        {
            new BulkUpdateBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> columnNames, SqlTransaction transaction = null)
        {
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
