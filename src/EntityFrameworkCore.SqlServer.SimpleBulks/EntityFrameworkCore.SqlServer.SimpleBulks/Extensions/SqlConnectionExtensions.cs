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
        public static void BulkInsert<T>(this IDbConnection connection, IList<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            var columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            BulkInsert(connection, data, tableName, columnNames);
        }

        public static void BulkUpdate<T>(this IDbConnection connection, IList<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            string idColumn = idSelector.Body.GetMemberName();
            var columnNames = columnNamesSelector.Body.GetMemberNames();
            BulkUpdate(connection, data, tableName, idColumn, columnNames.ToArray());
        }

        public static void BulkDelete<T>(this IDbConnection connection, IList<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            string idColumn = idSelector.Body.GetMemberName();
            BulkDelete(connection, data, tableName, idColumn);
        }

        public static void BulkInsert<T>(this IDbConnection connection, IList<T> data, string tableName, params string[] columnNames)
        {
            var dataTable = data.ToDataTable(columnNames.ToList());

            connection.Open();
            dataTable.SqlBulkCopy(tableName, connection as SqlConnection);
            connection.Close();
        }

        public static void BulkUpdate<T>(this IDbConnection connection, IList<T> data, string tableName, string idColumn, params string[] columnNames)
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNamesIncludeId = columnNames.Select(RemoveOperator).ToList();
            propertyNamesIncludeId.Add(idColumn);

            var dataTable = data.ToDataTable(propertyNamesIncludeId);
            string sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumn);

            StringBuilder updateStatementBuilder = new StringBuilder();
            updateStatementBuilder.AppendLine("update a set");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, columnNames.Select(CreateSetStatement)));
            updateStatementBuilder.AppendLine("from " + tableName + " a join [" + temptableName + "] b on a.[" + idColumn + "] = b.[" + idColumn + "]");

            connection.Open();

            using (var createTemptableCommand = connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, connection as SqlConnection);

            using (var updateCommand = connection.CreateCommand())
            {
                updateCommand.CommandText = updateStatementBuilder.ToString();
                var affectedRows = updateCommand.ExecuteNonQuery();
            }

            connection.Close();
        }

        public static void BulkDelete<T>(this IDbConnection connection, IList<T> data, string tableName, string idColumn)
        {
            var temptableName = "#" + Guid.NewGuid();
            var dataTable = data.ToDataTable(new List<string> { idColumn });
            string sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, idColumn);

            string deleteStatement = $"delete a from {tableName} a join [{temptableName}] b on a.[{idColumn}] = b.[{idColumn}]";

            connection.Open();

            using (var createTemptableCommand = connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, connection as SqlConnection);

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

            string statement = "a.[{0}] {1} b.[{0}]";

            return string.Format(statement, sqlProp, sqlOperator);
        }

        private static string RemoveOperator(string prop)
        {
            var rs = prop.Replace("+=", "");
            return rs;
        }
    }
}
