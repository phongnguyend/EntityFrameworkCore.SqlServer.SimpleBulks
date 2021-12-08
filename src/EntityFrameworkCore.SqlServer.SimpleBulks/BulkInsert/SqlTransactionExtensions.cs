using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert
{
    public static class SqlTransactionExtensions
    {
        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, IEnumerable<string> columnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName)
        {
            string tableName = TableMapper.Resolve(typeof(T));

            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNamesSelector)
                .ToTable(tableName)
                .WithOuputId(idSelector)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }

        public static void BulkInsert<T>(this SqlTransaction transaction, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames, string idColumnName)
        {
            new BulkInsertBuilder<T>(transaction)
                .WithData(data)
                .WithColumns(columnNames)
                .ToTable(tableName)
                .WithOuputId(idColumnName)
                .ConfigureBulkOptions(opt => opt.Timeout = 30)
                .Execute();
        }
    }
}
