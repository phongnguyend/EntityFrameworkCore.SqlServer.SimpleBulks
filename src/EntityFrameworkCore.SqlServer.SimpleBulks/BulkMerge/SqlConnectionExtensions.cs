using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge
{
    public static class SqlConnectionExtensions
    {
        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkMerge(data, tableName, idSelector, updateColumnNamesSelector, insertColumnNamesSelector);
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkMerge(data, tableName, idColumn, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            string tableName = TableMapper.Resolve(typeof(T));
            connection.BulkMerge(data, tableName, idColumns, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            var idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };

            var updateColumnNames = updateColumnNamesSelector.Body.GetMemberNames().ToArray();
            var insertColumnNames = insertColumnNamesSelector.Body.GetMemberNames().ToArray();

            connection.BulkMerge(data, tableName, idColumns, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            connection.BulkMerge(data, tableName, new List<string> { idColumn }, updateColumnNames, insertColumnNames);
        }

        public static void BulkMerge<T>(this SqlConnection connection, IEnumerable<T> data, string tableName, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames)
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNames = updateColumnNames.Select(RemoveOperator).ToList();
            propertyNames.AddRange(idColumns);
            propertyNames.AddRange(insertColumnNames);
            propertyNames = propertyNames.Distinct().ToList();

            var dataTable = data.ToDataTable(propertyNames);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumns);

            var mergeStatementBuilder = new StringBuilder();

            var joinCondition = string.Join(" and ", idColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {Constants.Collation}" : string.Empty;
                return $"s.[{x}]{collation} = t.[{x}]{collation}";
            }));

            mergeStatementBuilder.AppendLine($"MERGE {tableName} t");
            mergeStatementBuilder.AppendLine($"    USING [{temptableName}] s");
            mergeStatementBuilder.AppendLine($"ON ({joinCondition})");
            mergeStatementBuilder.AppendLine($"WHEN MATCHED");
            mergeStatementBuilder.AppendLine($"    THEN UPDATE SET");
            mergeStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, updateColumnNames.Select(x => "         " + CreateSetStatement(x, "t", "s"))));
            mergeStatementBuilder.AppendLine($"WHEN NOT MATCHED BY TARGET");
            mergeStatementBuilder.AppendLine($"    THEN INSERT ({string.Join(", ", insertColumnNames)})");
            mergeStatementBuilder.AppendLine($"         VALUES ({string.Join(", ", insertColumnNames.Select(x => $"s.{x}"))})");
            mergeStatementBuilder.AppendLine(";");

            connection.Open();

            using (var createTemptableCommand = connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, connection);

            using (var updateCommand = connection.CreateCommand())
            {
                updateCommand.CommandText = mergeStatementBuilder.ToString();
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
