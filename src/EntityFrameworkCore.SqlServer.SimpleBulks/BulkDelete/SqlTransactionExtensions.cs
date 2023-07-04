using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete
{
    public static class SqlTransactionExtensions
    {
        public static BulkDeleteResult BulkDelete<T>(this SqlTransaction transaction, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkDeleteBuilder<T>(transaction)
                .WithData(data)
                .WithId(idSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkDeleteResult BulkDelete<T>(this SqlTransaction transaction, IEnumerable<T> data, string idColumn, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkDeleteBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumn)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkDeleteResult BulkDelete<T>(this SqlTransaction transaction, IEnumerable<T> data, IEnumerable<string> idColumns, Action<BulkOptions> configureOptions = null)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            return new BulkDeleteBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumns)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkDeleteResult BulkDelete<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Action<BulkOptions> configureOptions = null)
        {
            return new BulkDeleteBuilder<T>(transaction)
                .WithData(data)
                .WithId(idSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkDeleteResult BulkDelete<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, string idColumn, Action<BulkOptions> configureOptions = null)
        {
            return new BulkDeleteBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumn)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }

        public static BulkDeleteResult BulkDelete<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, Action<BulkOptions> configureOptions = null)
        {
            return new BulkDeleteBuilder<T>(transaction)
                .WithData(data)
                .WithId(idColumns)
                .ToTable(tableName)
                .ConfigureBulkOptions(configureOptions)
                .Execute();
        }
    }
}
