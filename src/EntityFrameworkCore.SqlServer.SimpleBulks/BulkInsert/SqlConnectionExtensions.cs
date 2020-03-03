using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert
{
    public static class SqlConnectionExtensions
    {
        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkInsert(data, tableName, columnNamesSelector);
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> columnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkInsert(data, tableName, columnNames);
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            var columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            connection.BulkInsert(data, tableName, columnNames);
        }

        public static void BulkInsert<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> columnNames)
        {
            var dataTable = data.ToDataTable(columnNames.ToList());

            connection.Open();
            dataTable.SqlBulkCopy(tableName, connection);
            connection.Close();
        }
    }
}
