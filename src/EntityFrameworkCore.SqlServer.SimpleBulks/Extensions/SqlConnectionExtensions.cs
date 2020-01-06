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

        public static void BulkMerge<T>(this SqlConnection connection, IList<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            var idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };

            var updateColumnNames = updateColumnNamesSelector.Body.GetMemberNames().ToArray();
            var insertColumnNames = insertColumnNamesSelector.Body.GetMemberNames().ToArray();

            connection.BulkMerge(data, tableName, idColumns, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this SqlConnection connection, IList<T> data, string tableName, string idColumn, string[] updateColumnNames, string[] insertColumnNames)
        {
            connection.BulkMerge(data, tableName, new List<string> { idColumn }, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this SqlConnection connection, IList<T> data, string tableName, List<string> idColumns, string[] updateColumnNames, string[] insertColumnNames)
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNames = updateColumnNames.Select(RemoveOperator).ToList();
            propertyNames.AddRange(idColumns);
            propertyNames.AddRange(insertColumnNames);
            propertyNames = propertyNames.Distinct().ToList();

            var dataTable = data.ToDataTable(propertyNames);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumns);

            string mergeStatement = string.Empty;
            mergeStatement += $"MERGE {tableName} t";
            mergeStatement += Environment.NewLine;
            mergeStatement += $"    USING [{temptableName}] s";
            mergeStatement += Environment.NewLine;
            mergeStatement += $"ON ({string.Join(" and ", idColumns.Select(x => $"s.[{x}] = t.[{x}]"))})";
            mergeStatement += Environment.NewLine;
            mergeStatement += $"WHEN MATCHED";
            mergeStatement += Environment.NewLine;
            mergeStatement += $"    THEN UPDATE SET";
            mergeStatement += Environment.NewLine;
            mergeStatement += string.Join("," + Environment.NewLine, updateColumnNames.Select(x => "         " + CreateSetStatement(x, "t", "s")));
            mergeStatement += Environment.NewLine;
            mergeStatement += $"WHEN NOT MATCHED BY TARGET";
            mergeStatement += Environment.NewLine;
            mergeStatement += $"    THEN INSERT ({string.Join(", ", insertColumnNames)})";
            mergeStatement += Environment.NewLine;
            mergeStatement += $"         VALUES ({string.Join(", ", insertColumnNames.Select(x => $"s.{x}"))})";
            mergeStatement += ";";

            connection.Open();

            using (var createTemptableCommand = connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, connection);

            using (var updateCommand = connection.CreateCommand())
            {
                updateCommand.CommandText = mergeStatement;
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
