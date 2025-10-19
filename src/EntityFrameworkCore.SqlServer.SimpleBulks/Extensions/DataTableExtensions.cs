using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    public static async Task SqlBulkCopyAsync(this DataTable dataTable, string tableName, IReadOnlyDictionary<string, string> columnNameMappings, SqlConnection connection, SqlTransaction transaction, BulkOptions options = null, CancellationToken cancellationToken = default)
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

        await bulkCopy.WriteToServerAsync(dataTable, cancellationToken);
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
            return dataColumn.DataType.ToSqlType();
        }

        return columnTypeMappings.TryGetValue(dataColumn.ColumnName, out string value) ? value : dataColumn.DataType.ToSqlType();
    }
}
