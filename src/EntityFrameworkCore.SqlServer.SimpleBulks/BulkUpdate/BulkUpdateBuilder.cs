using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public class BulkUpdateBuilder<T>
{
    private TableInfor _table;
    private IEnumerable<string> _idColumns;
    private IEnumerable<string> _columnNames;
    private IDictionary<string, string> _dbColumnMappings;
    private BulkUpdateOptions _options;
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    public BulkUpdateBuilder(SqlConnection connection)
    {
        _connection = connection;
    }

    public BulkUpdateBuilder(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public BulkUpdateBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkUpdateBuilder<T> WithId(string idColumn)
    {
        _idColumns = new[] { idColumn };
        return this;
    }

    public BulkUpdateBuilder<T> WithId(IEnumerable<string> idColumns)
    {
        _idColumns = idColumns;
        return this;
    }

    public BulkUpdateBuilder<T> WithId(Expression<Func<T, object>> idSelector)
    {
        var idColumn = idSelector.Body.GetMemberName();
        _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
        return this;
    }

    public BulkUpdateBuilder<T> WithColumns(IEnumerable<string> columnNames)
    {
        _columnNames = columnNames;
        return this;
    }

    public BulkUpdateBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
    {
        _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkUpdateBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
    {
        _dbColumnMappings = dbColumnMappings;
        return this;
    }

    public BulkUpdateBuilder<T> ConfigureBulkOptions(Action<BulkUpdateOptions> configureOptions)
    {
        _options = new BulkUpdateOptions();
        if (configureOptions != null)
        {
            configureOptions(_options);
        }
        return this;
    }

    private string GetDbColumnName(string columnName)
    {
        if (_dbColumnMappings == null)
        {
            return columnName;
        }

        return _dbColumnMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    public BulkUpdateResult Execute(IEnumerable<T> data)
    {
        if (data.Count() == 1)
        {
            return SingleUpdate(data.First());
        }

        var temptableName = $"[#{Guid.NewGuid()}]";

        var propertyNamesIncludeId = _columnNames.Select(RemoveOperator).ToList();
        propertyNamesIncludeId.AddRange(_idColumns);

        var dataTable = data.ToDataTable(propertyNamesIncludeId);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName);

        var joinCondition = string.Join(" and ", _idColumns.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" COLLATE {_options.Collation}" : string.Empty;
            return $"a.[{GetDbColumnName(x)}]{collation} = b.[{x}]{collation}";
        }));

        var updateStatementBuilder = new StringBuilder();
        updateStatementBuilder.AppendLine("UPDATE a SET");
        updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _columnNames.Select(x => CreateSetStatement(x, "a", "b"))));
        updateStatementBuilder.AppendLine($"FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition);

        _connection.EnsureOpen();

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");
        using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
        {
            createTemptableCommand.ExecuteNonQuery();
        }
        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
        dataTable.SqlBulkCopy(temptableName, null, _connection, _transaction, _options);
        Log("End executing SqlBulkCopy.");

        var sqlUpdateStatement = updateStatementBuilder.ToString();

        Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");
        using var updateCommand = _connection.CreateTextCommand(_transaction, sqlUpdateStatement, _options);
        var affectedRows = updateCommand.ExecuteNonQuery();
        Log("End updating.");

        return new BulkUpdateResult
        {
            AffectedRows = affectedRows
        };
    }

    public BulkUpdateResult SingleUpdate(T dataToUpdate)
    {
        var whereCondition = string.Join(" AND ", _idColumns.Select(x =>
        {
            return CreateSetStatement(x);
        }));

        var updateStatementBuilder = new StringBuilder();
        updateStatementBuilder.AppendLine($"UPDATE {_table.SchemaQualifiedTableName} SET");
        updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _columnNames.Select(x => CreateSetStatement(x))));
        updateStatementBuilder.AppendLine($"WHERE {whereCondition}");

        var sqlUpdateStatement = updateStatementBuilder.ToString();

        var propertyNamesIncludeId = _columnNames.Select(RemoveOperator).ToList();
        propertyNamesIncludeId.AddRange(_idColumns);

        Log($"Begin updating:{Environment.NewLine}{sqlUpdateStatement}");

        using var updateCommand = _connection.CreateTextCommand(_transaction, sqlUpdateStatement, _options);

        dataToUpdate.ToSqlParameters(propertyNamesIncludeId).ForEach(x => updateCommand.Parameters.Add(x));

        _connection.EnsureOpen();

        var affectedRow = updateCommand.ExecuteNonQuery();

        Log($"End updating.");

        return new BulkUpdateResult
        {
            AffectedRows = affectedRow
        };
    }

    private string CreateSetStatement(string prop, string leftTable, string rightTable)
    {
        string sqlOperator = "=";
        string sqlProp = RemoveOperator(prop);

        if (prop.EndsWith("+="))
        {
            sqlOperator = "+=";
        }

        return $"{leftTable}.[{GetDbColumnName(sqlProp)}] {sqlOperator} {rightTable}.[{sqlProp}]";
    }

    private string CreateSetStatement(string prop)
    {
        string sqlOperator = "=";
        string sqlProp = RemoveOperator(prop);

        if (prop.EndsWith("+="))
        {
            sqlOperator = "+=";
        }

        return $"[{GetDbColumnName(sqlProp)}] {sqlOperator} @{sqlProp}";
    }

    private static string RemoveOperator(string prop)
    {
        var rs = prop.Replace("+=", "");
        return rs;
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkUpdate]: {message}");
    }
}
