using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public class BulkDeleteBuilder<T>
{
    private TableInfor _table;
    private IEnumerable<string> _idColumns;
    private IReadOnlyDictionary<string, string> _columnNameMappings;
    private IReadOnlyDictionary<string, string> _columnTypeMappings;
    private BulkDeleteOptions _options;
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    public BulkDeleteBuilder(SqlConnection connection)
    {
        _connection = connection;
    }

    public BulkDeleteBuilder(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public BulkDeleteBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkDeleteBuilder<T> WithId(string idColumn)
    {
        _idColumns = [ idColumn ];
        return this;
    }

    public BulkDeleteBuilder<T> WithId(IEnumerable<string> idColumns)
    {
        _idColumns = idColumns;
        return this;
    }

    public BulkDeleteBuilder<T> WithId(Expression<Func<T, object>> idSelector)
    {
        var idColumn = idSelector.Body.GetMemberName();
        _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
        return this;
    }

    public BulkDeleteBuilder<T> WithDbColumnMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public BulkDeleteBuilder<T> WithDbColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public BulkDeleteBuilder<T> ConfigureBulkOptions(Action<BulkDeleteOptions> configureOptions)
    {
        _options = new BulkDeleteOptions();
        if (configureOptions != null)
        {
            configureOptions(_options);
        }
        return this;
    }

    private string GetDbColumnName(string columnName)
    {
        if (_columnNameMappings == null)
        {
            return columnName;
        }

        return _columnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
    }

    public BulkDeleteResult Execute(IEnumerable<T> data)
    {
        if (data.Count() == 1)
        {
            return SingleDelete(data.First());
        }

        var temptableName = $"[#{Guid.NewGuid()}]";
        var dataTable = data.ToDataTable(_idColumns);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _columnTypeMappings);

        var joinCondition = string.Join(" AND ", _idColumns.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" COLLATE {_options.Collation}" : string.Empty;
            return $"a.[{GetDbColumnName(x)}]{collation} = b.[{x}]{collation}";
        }));

        var deleteStatement = $"DELETE a FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition;

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

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

        var affectedRows = deleteCommand.ExecuteNonQuery();

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }

    public BulkDeleteResult SingleDelete(T dataToDelete)
    {
        var whereCondition = string.Join(" AND ", _idColumns.Select(x =>
        {
            return $"[{GetDbColumnName(x)}] = @{x}";
        }));

        var deleteStatement = $"DELETE FROM {_table.SchemaQualifiedTableName} WHERE " + whereCondition;

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement, _options);

        dataToDelete.ToSqlParameters(_idColumns).ForEach(x => deleteCommand.Parameters.Add(x));

        _connection.EnsureOpen();

        var affectedRows = deleteCommand.ExecuteNonQuery();

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkDelete]: {message}");
    }
}
