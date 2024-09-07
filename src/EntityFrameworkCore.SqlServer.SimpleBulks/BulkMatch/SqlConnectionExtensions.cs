using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch
{
    public static class SqlConnectionExtensions
    {
        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkMatchBuilder<T>(connection)
                 .WithReturnedColumns(returnedColumnsSelector)
                 .WithTable(tableName)
                 .WithMatchedColumns(matchedColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string matchedColumn, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkMatchBuilder<T>(connection)
                .WithReturnedColumns(returnedColumns)
                .WithTable(tableName)
                .WithMatchedColumn(matchedColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkMatchBuilder<T>(connection)
                .WithReturnedColumns(returnedColumns)
                .WithTable(tableName)
                .WithMatchedColumns(matchedColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string tableName, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
        {
            return new BulkMatchBuilder<T>(connection)
                 .WithReturnedColumns(returnedColumnsSelector)
                 .WithTable(tableName)
                 .WithMatchedColumns(matchedColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string tableName, string matchedColumns, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            return new BulkMatchBuilder<T>(connection)
                .WithReturnedColumns(returnedColumns)
                .WithTable(tableName)
                .WithMatchedColumn(matchedColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string tableName, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            return new BulkMatchBuilder<T>(connection)
                .WithReturnedColumns(returnedColumns)
                .WithTable(tableName)
                .WithMatchedColumns(matchedColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }
    }
}
