using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public class BulkDeleteBuilder<T>
{
    private TableInfor<T> _table;
    private IReadOnlyCollection<string> _deleteKeys;
    private BulkDeleteOptions _options = BulkDeleteOptions.DefaultOptions;
    private readonly ConnectionContext _connectionContext;

    public BulkDeleteBuilder(ConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public BulkDeleteBuilder<T> ToTable(TableInfor<T> table)
    {
        _table = table;
        return this;
    }

    public BulkDeleteBuilder<T> WithId(IReadOnlyCollection<string> idColumns)
    {
        _deleteKeys = idColumns;
        return this;
    }

    public BulkDeleteBuilder<T> WithId(Expression<Func<T, object>> idSelector)
    {
        var idColumn = idSelector.Body.GetMemberName();
        _deleteKeys = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
        return this;
    }

    public BulkDeleteBuilder<T> WithBulkOptions(BulkDeleteOptions options)
    {
        _options = options ?? BulkDeleteOptions.DefaultOptions;
        return this;
    }

    private IReadOnlyCollection<string> GetKeys()
    {
        return _table.IncludeDiscriminator(_deleteKeys);
    }

    private string CreateJoinCondition(DataTable dataTable)
    {
        var keys = GetKeys();

        return string.Join(" AND ", keys.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" COLLATE {_options.Collation}" : string.Empty;
            return $"a.[{_table.GetDbColumnName(x)}]{collation} = b.[{x}]{collation}";
        }));
    }

    private string CreateWhereCondition()
    {
        var keys = GetKeys();

        return string.Join(" AND ", keys.Select(x =>
        {
            return $"[{_table.GetDbColumnName(x)}] = {_table.CreateParameterName(x)}";
        }));
    }

    public BulkDeleteResult Execute(IReadOnlyCollection<T> data)
    {
        if (data.Count == 1)
        {
            return SingleDelete(data.First());
        }

        var temptableName = $"[#{Guid.NewGuid()}]";
        var dataTable = data.ToDataTable(_deleteKeys, valueConverters: _table.ValueConverters, discriminator: _table.Discriminator);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

        var joinCondition = CreateJoinCondition(dataTable);

        var deleteStatement = $"DELETE a FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition;

        _connectionContext.EnsureOpen();

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

        using (var createTemptableCommand = _connectionContext.CreateTextCommand(sqlCreateTemptable, _options))
        {
            createTemptableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");


        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");

        _connectionContext.SqlBulkCopy(dataTable, temptableName, null, _options);

        Log("End executing SqlBulkCopy.");

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connectionContext.CreateTextCommand(deleteStatement, _options);

        var affectedRows = deleteCommand.ExecuteNonQuery();

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }

    public BulkDeleteResult SingleDelete(T dataToDelete)
    {
        var whereCondition = CreateWhereCondition();

        var deleteStatement = $"DELETE FROM {_table.SchemaQualifiedTableName} WHERE " + whereCondition;

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connectionContext.CreateTextCommand(deleteStatement, _options);

        LogParameters(_table.CreateSqlParameters(deleteCommand, dataToDelete, _deleteKeys, includeDiscriminator: true, autoAdd: true));

        _connectionContext.EnsureOpen();

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

    private void LogParameters(List<ParameterInfo> parameters)
    {
        if (_options?.LogTo == null)
        {
            return;
        }

        foreach (var parameter in parameters)
        {
            _options.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkDelete][Parameter]: {parameter}");
        }
    }

    public async Task<BulkDeleteResult> ExecuteAsync(IReadOnlyCollection<T> data, CancellationToken cancellationToken = default)
    {
        if (data.Count == 1)
        {
            return await SingleDeleteAsync(data.First(), cancellationToken);
        }

        var temptableName = $"[#{Guid.NewGuid()}]";
        var dataTable = await data.ToDataTableAsync(_deleteKeys, valueConverters: _table.ValueConverters, discriminator: _table.Discriminator, cancellationToken: cancellationToken);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

        var joinCondition = CreateJoinCondition(dataTable);

        var deleteStatement = $"DELETE a FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition;

        await _connectionContext.EnsureOpenAsync(cancellationToken);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");

        using (var createTemptableCommand = _connectionContext.CreateTextCommand(sqlCreateTemptable, _options))
        {
            await createTemptableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");

        await _connectionContext.SqlBulkCopyAsync(dataTable, temptableName, null, _options, cancellationToken);

        Log("End executing SqlBulkCopy.");

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connectionContext.CreateTextCommand(deleteStatement, _options);

        var affectedRows = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }

    public async Task<BulkDeleteResult> SingleDeleteAsync(T dataToDelete, CancellationToken cancellationToken = default)
    {
        var whereCondition = CreateWhereCondition();

        var deleteStatement = $"DELETE FROM {_table.SchemaQualifiedTableName} WHERE " + whereCondition;

        Log($"Begin deleting:{Environment.NewLine}{deleteStatement}");

        using var deleteCommand = _connectionContext.CreateTextCommand(deleteStatement, _options);

        LogParameters(_table.CreateSqlParameters(deleteCommand, dataToDelete, _deleteKeys, includeDiscriminator: true, autoAdd: true));

        await _connectionContext.EnsureOpenAsync(cancellationToken);

        var affectedRows = await deleteCommand.ExecuteNonQueryAsync(cancellationToken);

        Log("End deleting.");

        return new BulkDeleteResult
        {
            AffectedRows = affectedRows
        };
    }
}
