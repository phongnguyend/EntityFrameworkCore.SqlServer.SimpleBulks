using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate
{
    public static class SqlConnectionExtensions
    {
        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkUpdate(data, tableName, idSelector, columnNamesSelector);
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkUpdate(data, tableName, idColumn, columnNames);
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkUpdate(data, tableName, idColumns, columnNames);
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            var idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
            var columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();

            connection.BulkUpdate(data, tableName, idColumns, columnNames);
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> columnNames)
        {
            connection.BulkUpdate(data, tableName, new List<string> { idColumn }, columnNames);
        }

        public static void BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> columnNames)
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNamesIncludeId = columnNames.Select(RemoveOperator).ToList();
            propertyNamesIncludeId.AddRange(idColumns);

            var dataTable = data.ToDataTable(propertyNamesIncludeId);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumns);

            var updateStatementBuilder = new StringBuilder();
            updateStatementBuilder.AppendLine("update a set");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, columnNames.Select(x => CreateSetStatement(x, "a", "b"))));
            updateStatementBuilder.AppendLine($"from {tableName } a join [{ temptableName}] b on " + string.Join(" and ", idColumns.Select(x => $"a.[{x}] = b.[{x}]")));

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

        private static string CreateSetStatement(string prop, string leftTable, string rightTable)
        {
            string sqlOperator = "=";
            string sqlProp = RemoveOperator(prop);

            if (prop.EndsWith("+="))
            {
                sqlOperator = "+=";
            }

            return $"{leftTable}.[{sqlProp}] {sqlOperator} {rightTable}.[{sqlProp}]";
        }

        private static string RemoveOperator(string prop)
        {
            var rs = prop.Replace("+=", "");
            return rs;
        }
    }
}
