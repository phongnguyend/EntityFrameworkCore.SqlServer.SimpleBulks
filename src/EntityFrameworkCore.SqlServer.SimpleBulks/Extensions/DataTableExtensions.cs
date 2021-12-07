using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class DataTableExtensions
    {
        public static string GenerateTableDefinition(this DataTable table, string tableName, IEnumerable<string> idColumns = null)
        {
            var sql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", tableName);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                sql.Append($"\n\t[{table.Columns[i].ColumnName}]");
                var isId = idColumns != null && idColumns.Contains(table.Columns[i].ColumnName);
                var sqlType = table.Columns[i].DataType.ToSqlType(isId);
                sql.Append($" {sqlType}");
                sql.Append(isId ? " NOT NULL" : " NULL");
                sql.Append(",");
            }

            if (idColumns != null && idColumns.Any())
            {
                var key = string.Join(", ", idColumns.Select(x => $"[{x}]"));
                sql.Append($"\n\tPRIMARY KEY ({key})");
            }

            sql.Append("\n);");

            return sql.ToString();
        }

        public static void SqlBulkCopy(this DataTable dataTable, string tableName, SqlConnection connection, SqlTransaction transaction, BulkOptions options = null)
        {
            options ??= new BulkOptions()
            {
                BatchSize = 0,
                Timeout = 30,
            };

            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                BatchSize = options.BatchSize,
                BulkCopyTimeout = options.Timeout,
                DestinationTableName = $"[{ tableName }]"
            };

            foreach (DataColumn dtColum in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(dtColum.ColumnName, dtColum.ColumnName);
            }

            bulkCopy.WriteToServer(dataTable);
        }
    }
}
