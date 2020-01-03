using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class SqlConnectionExtensions
    {
        public static void BulkInsert<T>(this SqlConnection connection, IList<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            var columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            connection.BulkInsert(data, tableName, columnNames);
        }

        public static void BulkInsert<T>(this SqlConnection connection, IList<T> data, string tableName, params string[] columnNames)
        {
            var dataTable = data.ToDataTable(columnNames.ToList());

            connection.Open();
            dataTable.SqlBulkCopy(tableName, connection);
            connection.Close();
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IList<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            var idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
            var columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();

            connection.BulkUpdate(data, tableName, idColumns, columnNames);
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IList<T> data, string tableName, string idColumn, params string[] columnNames)
        {
            connection.BulkUpdate(data, tableName, new List<string> { idColumn }, columnNames);
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IList<T> data, string tableName, List<string> idColumns, params string[] columnNames)
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNamesIncludeId = columnNames.Select(RemoveOperator).ToList();
            propertyNamesIncludeId.AddRange(idColumns);

            var dataTable = data.ToDataTable(propertyNamesIncludeId);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumns);

            var updateStatementBuilder = new StringBuilder();
            updateStatementBuilder.AppendLine("update a set");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, columnNames.Select(CreateSetStatement)));
            updateStatementBuilder.AppendLine("from " + tableName + " a join [" + temptableName + "] b on " + string.Join(" and ", idColumns.Select(x => $"a.[{x}] = b.[{x}]")));

            connection.Open();

            using (var createTemptableCommand = connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, connection);

            using (var updateCommand = connection.CreateCommand())
            {
                updateCommand.CommandText = updateStatementBuilder.ToString();
                var affectedRows = updateCommand.ExecuteNonQuery();
            }

            connection.Close();
        }

        public static void BulkDelete<T>(this SqlConnection connection, IList<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            var idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };

            connection.BulkDelete(data, tableName, idColumns);
        }

        public static void BulkDelete<T>(this SqlConnection connection, IList<T> data, string tableName, string idColumn)
        {
            connection.BulkDelete(data, tableName, new List<string> { idColumn });
        }

        public static void BulkDelete<T>(this SqlConnection connection, IList<T> data, string tableName, List<string> idColumns)
        {
            var temptableName = "#" + Guid.NewGuid();
            var dataTable = data.ToDataTable(idColumns);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumns);
            var deleteStatement = $"delete a from {tableName} a join [{temptableName}] b on " + string.Join(" and ", idColumns.Select(x => $"a.[{x}] = b.[{x}]"));

            connection.Open();

            using (var createTemptableCommand = connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, connection);

            using (var deleteCommand = connection.CreateCommand())
            {
                deleteCommand.CommandText = deleteStatement.ToString();
                var affectedRows = deleteCommand.ExecuteNonQuery();
            }

            connection.Close();
        }

        private static string CreateSetStatement(string prop)
        {
            string sqlOperator = "=";
            string sqlProp = RemoveOperator(prop);

            if (prop.EndsWith("+="))
            {
                sqlOperator = "+=";
            }

            return $"a.[{sqlProp}] {sqlOperator} b.[{sqlProp}]";
        }

        private static string RemoveOperator(string prop)
        {
            var rs = prop.Replace("+=", "");
            return rs;
        }
    }
}
