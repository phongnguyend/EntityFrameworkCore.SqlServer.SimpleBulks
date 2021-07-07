using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete
{
    public static class SqlConnectionExtensions
    {
        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkDelete(data, tableName, idSelector);
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkDelete(data, tableName, idColumn);
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkDelete(data, tableName, idColumns);
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            var idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };

            connection.BulkDelete(data, tableName, idColumns);
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, string idColumn)
        {
            connection.BulkDelete(data, tableName, new List<string> { idColumn });
        }

        public static void BulkDelete<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns)
        {
            var temptableName = "#" + Guid.NewGuid();
            var dataTable = data.ToDataTable(idColumns);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumns);

            var joinCondition = string.Join(" and ", idColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {Constants.Collation}" : string.Empty;
                return $"a.[{x}]{collation} = b.[{x}]{collation}";
            }));

            var deleteStatement = $"delete a from {tableName} a join [{temptableName}] b on " + joinCondition;

            connection.Open();

            using (var createTemptableCommand = connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, connection);

            using (var deleteCommand = connection.CreateCommand())
            {
                deleteCommand.CommandText = deleteStatement;
                var affectedRows = deleteCommand.ExecuteNonQuery();
            }

            connection.Close();
        }
    }
}
