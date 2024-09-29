﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate
{
    public static class SqlConnectionExtensions
    {
        public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkUpdateBuilder<T>(connection)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute(data);
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkUpdateBuilder<T>(connection)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute(data);
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkUpdateBuilder<T>(connection)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute(data);
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
        {
            return new BulkUpdateBuilder<T>(connection)
                .WithId(idSelector)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute(data);
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
        {
            return new BulkUpdateBuilder<T>(connection)
                .WithId(idColumn)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute(data);
        }

        public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
        {
            return new BulkUpdateBuilder<T>(connection)
                .WithId(idColumns)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute(data);
        }
    }
}
