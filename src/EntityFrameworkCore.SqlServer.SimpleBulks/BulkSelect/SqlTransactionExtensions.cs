using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkSelect
{
    public static class SqlTransactionExtensions
    {
        public static IEnumerable<T> BulkSelect<T>(this SqlTransaction transaction, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> matchColumnsSelector, IEnumerable<T> machedValues, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkSelectBuilder<T>(transaction)
                 .WithColumns(columnNamesSelector)
                 .FromTable(tableName)
                 .WithMatchedColumns(matchColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static IEnumerable<T> BulkSelect<T>(this SqlTransaction transaction, IEnumerable<string> columnNames, string matchColumn, IEnumerable<T> machedValues, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkSelectBuilder<T>(transaction)
                .WithColumns(columnNames)
                .FromTable(tableName)
                .WithMatchedColumns(matchColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static IEnumerable<T> BulkSelect<T>(this SqlTransaction transaction, IEnumerable<string> columnNames, IEnumerable<string> matchColumns, IEnumerable<T> machedValues, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkSelectBuilder<T>(transaction)
                .WithColumns(columnNames)
                .FromTable(tableName)
                .WithMatchedColumns(matchColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static IEnumerable<T> BulkSelect<T>(this SqlTransaction transaction, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> matchColumnsSelector, IEnumerable<T> machedValues, Action<BulkOptions> configureOptions = null)
        {
            return new BulkSelectBuilder<T>(transaction)
                 .WithColumns(columnNamesSelector)
                 .FromTable(tableName)
                 .WithMatchedColumns(matchColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static IEnumerable<T> BulkSelect<T>(this SqlTransaction transaction, string tableName, IEnumerable<string> columnNames, string matchColumn, IEnumerable<T> machedValues, Action<BulkOptions> configureOptions = null)
        {
            return new BulkSelectBuilder<T>(transaction)
                .WithColumns(columnNames)
                .FromTable(tableName)
                .WithMatchedColumns(matchColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static IEnumerable<T> BulkSelect<T>(this SqlTransaction transaction, string tableName, IEnumerable<string> columnNames, IEnumerable<string> matchColumns, IEnumerable<T> machedValues, Action<BulkOptions> configureOptions = null)
        {
            return new BulkSelectBuilder<T>(transaction)
                .WithColumns(columnNames)
                .FromTable(tableName)
                .WithMatchedColumns(matchColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }
    }
}
