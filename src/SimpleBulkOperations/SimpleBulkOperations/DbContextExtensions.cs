using Microsoft.EntityFrameworkCore;
using SimpleBulkOperations.SqlTypeConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SimpleBulkOperations
{
    public static class DbContextExtensions
    {
        public static void BulkInsert<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> columnNamesSelector)
        {
            var columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            BulkInsert(dbContext, data, tableName, columnNames);
        }

        public static void BulkUpdate<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector)
        {
            string idColumn = idSelector.Body.GetMemberName();
            var columnNames = columnNamesSelector.Body.GetMemberNames();
            BulkUpdate(dbContext, data, tableName, idColumn, columnNames.ToArray());
        }

        public static void BulkDelete<T>(this DbContext dbContext, IList<T> data, string tableName, Expression<Func<T, object>> idSelector)
        {
            string idColumn = idSelector.Body.GetMemberName();
            BulkDelete(dbContext, data, tableName, idColumn);
        }

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

        public static void BulkDelete<T>(this DbContext dbContext, IList<T> data, string tableName, string idColumn)
        {
            var temptableName = "#" + Guid.NewGuid();
            var dataTable = ToDataTable(data, new List<string> { idColumn });
            string sqlCreateTemptable = GetCreateTableSql(dataTable, temptableName, idColumn);

            string deleteStatement = $"delete a from {tableName} a join [{temptableName}] b on a.[{idColumn}] = b.[{idColumn}]";

            var connection = dbContext.Database.GetDbConnection();
            connection.Open();

            using (SqlCommand createTemptableCommand = new SqlCommand(sqlCreateTemptable, connection as SqlConnection))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            SqlBulkCopy(temptableName, dataTable, connection as SqlConnection);

            var affectedRows = dbContext.Database.ExecuteSqlCommand(deleteStatement);
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

                var sqlType = SqlTypeConverterFactory.GetConverter(table.Columns[i].DataType).Convert(table.Columns[i].DataType);
                sql.Append($" {sqlType}");
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
