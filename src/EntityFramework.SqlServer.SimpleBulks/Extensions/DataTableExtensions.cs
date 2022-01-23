using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace EntityFramework.SqlServer.SimpleBulks.Extensions
{
    public static class DataTableExtensions
    {
        public static string GenerateTableDefinition(this DataTable table, string tableName)
        {
            var sql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", tableName);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                sql.Append($"\n\t[{table.Columns[i].ColumnName}]");
                var sqlType = table.Columns[i].DataType.ToSqlType();
                sql.Append($" {sqlType} NULL");
                sql.Append(",");
            }

            sql.Append("\n);");

            return sql.ToString();
        }

        public static void SqlBulkCopy(this DataTable dataTable, string tableName, IDictionary<string, string> dbColumnMappings, SqlConnection connection, SqlTransaction transaction, BulkOptions options = null)
        {
            options = options ?? new BulkOptions()
            {
                BatchSize = 0,
                Timeout = 30,
            };

            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.BatchSize = options.BatchSize;
                bulkCopy.BulkCopyTimeout = options.Timeout;
                bulkCopy.DestinationTableName = $"[{ tableName }]";

                foreach (DataColumn dtColum in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(dtColum.ColumnName, GetDbColumnName(dtColum.ColumnName, dbColumnMappings));
                }

                bulkCopy.WriteToServer(dataTable);
            }


        }

        private static string GetDbColumnName(string columName, IDictionary<string, string> dbColumnMappings)
        {
            if (dbColumnMappings == null)
            {
                return columName;
            }

            return dbColumnMappings.ContainsKey(columName) ? dbColumnMappings[columName] : columName;
        }
    }
}
