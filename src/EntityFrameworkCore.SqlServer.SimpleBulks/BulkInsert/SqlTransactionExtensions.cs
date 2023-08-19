using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert
{
    public static class SqlTransactionExtensions
    {
        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
