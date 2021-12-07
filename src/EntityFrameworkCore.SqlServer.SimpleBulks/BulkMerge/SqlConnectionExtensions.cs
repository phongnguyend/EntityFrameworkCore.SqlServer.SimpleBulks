using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge
{
    public static class SqlConnectionExtensions
    {
        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithUpdateColumns(updateColumnNamesSelector)
                .WithInsertColumns(insertColumnNamesSelector)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, SqlTransaction transaction = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, SqlTransaction transaction = null)
        {
            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithUpdateColumns(updateColumnNamesSelector)
                .WithInsertColumns(insertColumnNamesSelector)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, SqlTransaction transaction = null)
        {
            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, SqlTransaction transaction = null)
        {
            new BulkMergeBuilder<T>(connection, transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithUpdateColumns(updateColumnNames)
                .WithInsertColumns(insertColumnNames)
                .ToTable(tableName)
                .ConfigreBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }
    }
}
