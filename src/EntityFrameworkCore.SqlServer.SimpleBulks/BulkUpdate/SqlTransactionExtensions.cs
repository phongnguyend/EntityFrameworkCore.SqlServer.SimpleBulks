using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate
{
    public static class SqlTransactionExtensions
    {
        public static BulkUpdateResult BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, Action<BulkOptions> configureOptions = null)
        {
            return new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> columnNames, Action<BulkOptions> configureOptions = null)
        {
            return new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> columnNames, Action<BulkOptions> configureOptions = null)
        {
            return new BulkUpdateBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
