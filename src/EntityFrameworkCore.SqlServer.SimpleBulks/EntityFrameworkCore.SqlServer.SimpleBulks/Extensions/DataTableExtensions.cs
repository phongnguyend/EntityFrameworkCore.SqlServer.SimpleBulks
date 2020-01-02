using EntityFrameworkCore.SqlServer.SimpleBulks.SqlTypeConverters;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class DataTableExtensions
    {
        public static string GenerateTableDefinition(this DataTable table, string tableName, string idColumn)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", tableName);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                sql.AppendFormat("\n\t[{0}]", table.Columns[i].ColumnName);

                var sqlType = SqlTypeConverterFactory.GetConverter(table.Columns[i].DataType).Convert(table.Columns[i].DataType);
                sql.Append($" {sqlType}");
                sql.Append(table.Columns[i].ColumnName == idColumn ? " NOT NULL" : " NULL");
                sql.Append(",");
            }
            sql.AppendFormat("PRIMARY KEY ({0})", idColumn);

            sql.Append("\n);");

            return sql.ToString();
        }

        public static void SqlBulkCopy(this DataTable dataTable, string tableName, SqlConnection connection)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            {
                bulkCopy.BulkCopyTimeout = 0;
                bulkCopy.DestinationTableName = "[" + tableName + "]";
                foreach (DataColumn dtColum in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(dtColum.ColumnName, dtColum.ColumnName);
                }

                bulkCopy.WriteToServer(dataTable);
            }
        }
    }
}
