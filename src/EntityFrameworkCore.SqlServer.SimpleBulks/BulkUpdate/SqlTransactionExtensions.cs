using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate
{
    public static class SqlTransactionExtensions
    {
        public static void BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> columnNames)
        {
            new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }
    }
}
