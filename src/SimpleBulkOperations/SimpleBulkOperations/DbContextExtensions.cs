using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SimpleBulkOperations
{
    public static class DbContextExtensions
    {
        public static void BulkInsert<T>(this DbContext dbContext, IList<T> data, string tableName, params string[] columnNames)
        {
            var dataTable = ToDataTable(data, columnNames.ToList());
            var connection = dbContext.Database.GetDbConnection();
            connection.Open();

            SqlBulkCopy(tableName, dataTable, connection as SqlConnection);

            connection.Close();
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> data, string tableName, string idColumn, params string[] columnNames)
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNamesIncludeId = columnNames.Select(RemoveOperator).ToList();
            propertyNamesIncludeId.Add(idColumn);

            var dataTable = ToDataTable(data, propertyNamesIncludeId);
            string sqlCreateTemptable = GetCreateTableSql(dataTable, temptableName, idColumn);

            StringBuilder updateStatementBuilder = new StringBuilder();
            updateStatementBuilder.AppendLine("update a set");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, columnNames.Select(CreateSetStatement)));
            updateStatementBuilder.AppendLine("from " + tableName + " a join [" + temptableName + "] b on a.[" + idColumn + "] = b.[" + idColumn + "]");

            var connection = dbContext.Database.GetDbConnection();
            connection.Open();

            using (SqlCommand createTemptableCommand = new SqlCommand(sqlCreateTemptable, connection as SqlConnection))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            SqlBulkCopy(temptableName, dataTable, connection as SqlConnection);

            var affectedRows = dbContext.Database.ExecuteSqlCommand(updateStatementBuilder.ToString());
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

        private static void SqlBulkCopy(string tableName, DataTable dataTable, SqlConnection connection)
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

        private static string GetCreateTableSql(DataTable table, string tableName, string idColumn)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendFormat("CREATE TABLE [{0}] (", tableName);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                sql.AppendFormat("\n\t[{0}]", table.Columns[i].ColumnName);

                switch (table.Columns[i].DataType.ToString().ToUpper())
                {
                    case "SYSTEM.GUID":
                        sql.Append(" uniqueidentifier");
                        break;
                    case "SYSTEM.BOOLEAN":
                        sql.Append(" bit");
                        break;
                    case "SYSTEM.INT16":
                        sql.Append(" smallint");
                        break;
                    case "SYSTEM.INT32":
                        sql.Append(" int");
                        break;
                    case "SYSTEM.INT64":
                        sql.Append(" bigint");
                        break;
                    case "SYSTEM.DATETIME":
                        sql.Append(" datetime");
                        break;
                    case "SYSTEM.STRING":
                        sql.Append(" nvarchar(max)");
                        break;
                    case "SYSTEM.SINGLE":
                        sql.Append(" single");
                        break;
                    case "SYSTEM.DOUBLE":
                        sql.Append(" double");
                        break;
                    case "SYSTEM.DECIMAL":
                        sql.Append(" decimal(38, 20)");
                        break;
                    default:
                        sql.Append(" nvarchar(max)");
                        break;
                }
                sql.Append(table.Columns[i].ColumnName == idColumn ? " NOT NULL" : " NULL");

                sql.Append(",");
            }
            sql.AppendFormat("PRIMARY KEY ({0})", idColumn);

            sql.Append("\n);");

            return sql.ToString();
        }

        private static DataTable ToDataTable<T>(IList<T> data, List<string> propertyNames)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));

            var updatablePros = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor prop in properties)
            {
                if (propertyNames.Contains(prop.Name))
                {
                    updatablePros.Add(prop);
                }
            }

            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in updatablePros)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in updatablePros)
                {
                    var value = prop.GetValue(item) ?? DBNull.Value;
                    row[prop.Name] = value;
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
