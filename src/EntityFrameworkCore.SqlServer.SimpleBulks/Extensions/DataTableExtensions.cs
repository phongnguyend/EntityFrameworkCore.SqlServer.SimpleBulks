﻿using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class DataTableExtensions
{
    public static string GenerateTableDefinition(this DataTable table, string tableName, IReadOnlyDictionary<string, string> columnNameMappings = null)
    {
        var sql = new StringBuilder();

        sql.AppendFormat("CREATE TABLE {0} (", tableName);

        for (int i = 0; i < table.Columns.Count; i++)
        {
            sql.Append($"\n\t[{GetDbColumnName(table.Columns[i].ColumnName, columnNameMappings)}]");
            var sqlType = table.Columns[i].DataType.ToSqlType();
            sql.Append($" {sqlType} NULL");
            sql.Append(",");
        }

        sql.Append("\n);");

        return sql.ToString();
    }

    public static void SqlBulkCopy(this DataTable dataTable, string tableName, IReadOnlyDictionary<string, string> columnNameMappings, SqlConnection connection, SqlTransaction transaction, BulkOptions options = null)
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
            DestinationTableName = $"{tableName}"
        };

        foreach (DataColumn dtColum in dataTable.Columns)
        {
            bulkCopy.ColumnMappings.Add(dtColum.ColumnName, GetDbColumnName(dtColum.ColumnName, columnNameMappings));
        }

        bulkCopy.WriteToServer(dataTable);
    }

    private static string GetDbColumnName(string columnName, IReadOnlyDictionary<string, string> columnNameMappings)
    {
        if (columnNameMappings == null)
        {
            return columnName;
        }

        return columnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }
}
