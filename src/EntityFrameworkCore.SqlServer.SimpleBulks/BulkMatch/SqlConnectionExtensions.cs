using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch
{
    public static class SqlConnectionExtensions
    {
        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, Expression<Func<T, object>> matchColumnsSelector, Expression<Func<T, object>> returnColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkMatchBuilder<T>(connection)
                 .WithColumns(returnColumnsSelector)
                 .WithTable(tableName)
                 .WithMatchedColumns(matchColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string matchColumn, IEnumerable<string> returnColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkMatchBuilder<T>(connection)
                .WithReturnColumns(returnColumns)
                .WithTable(tableName)
                .WithMatchedColumns(matchColumn)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, IEnumerable<string> matchColumns, IEnumerable<string> returnColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkMatchBuilder<T>(connection)
                .WithReturnColumns(returnColumns)
                .WithTable(tableName)
                .WithMatchedColumns(matchColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string tableName, Expression<Func<T, object>> matchColumnsSelector, Expression<Func<T, object>> returnColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
        {
            return new BulkMatchBuilder<T>(connection)
                 .WithColumns(returnColumnsSelector)
                 .WithTable(tableName)
                 .WithMatchedColumns(matchColumnsSelector)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string tableName, string matchColumns, IEnumerable<string> returnColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            return new BulkMatchBuilder<T>(connection)
                .WithReturnColumns(returnColumns)
                .WithTable(tableName)
                .WithMatchedColumns(matchColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }

        public static List<T> BulkMatch<T>(this SqlConnection connection, IEnumerable<T> machedValues, string tableName, IEnumerable<string> matchColumns, IEnumerable<string> returnColumns, Action<BulkMatchOptions> configureOptions = null)
        {
            return new BulkMatchBuilder<T>(connection)
                .WithReturnColumns(returnColumns)
                .WithTable(tableName)
                .WithMatchedColumns(matchColumns)
                .ConfigureBulkOptions(configureOptions)
                .Execute(machedValues);
        }
    }
}
