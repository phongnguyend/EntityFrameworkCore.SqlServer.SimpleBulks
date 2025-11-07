using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class DataTableExtensions
{
    public static string GenerateTableDefinition(this DataTable table, string tableName,
        IReadOnlyDictionary<string, string> columnNameMappings,
        IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        var sql = new StringBuilder();

        sql.AppendFormat("CREATE TABLE {0} (", tableName);

        for (int i = 0; i < table.Columns.Count; i++)
        {
            sql.Append($"\n\t[{GetDbColumnName(table.Columns[i].ColumnName, columnNameMappings)}]");
            var sqlType = GetDbColumnType(table.Columns[i], columnTypeMappings);
            sql.Append($" {sqlType} NULL");
            sql.Append(",");
        }

        sql.Append("\n);");

        return sql.ToString();
    }

    private static string GetDbColumnName(string columnName, IReadOnlyDictionary<string, string> columnNameMappings)
    {
        if (columnNameMappings == null)
        {
            return columnName;
        }

        return columnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    private static string GetDbColumnType(DataColumn dataColumn, IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        if (columnTypeMappings == null)
        {
            return dataColumn.DataType.ToSqlDbType();
        }

        return columnTypeMappings.TryGetValue(dataColumn.ColumnName, out string value) ? value : dataColumn.DataType.ToSqlDbType();
    }
}
