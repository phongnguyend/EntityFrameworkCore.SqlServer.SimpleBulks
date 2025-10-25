using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public class TempTableBuilder<T>
{
    private IEnumerable<T> _data;
    private IEnumerable<string> _columnNames;
    private IReadOnlyDictionary<string, string> _columnNameMappings;
    private IReadOnlyDictionary<string, string> _columnTypeMappings;
    private IReadOnlyDictionary<string, ValueConverter> _valueConverters;
    private TempTableOptions _options;
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    public TempTableBuilder(SqlConnection connection)
    {
        _connection = connection;
    }

    public TempTableBuilder(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public TempTableBuilder<T> WithData(IEnumerable<T> data)
    {
        _data = data;
        return this;
    }

    public TempTableBuilder<T> WithColumns(IEnumerable<string> columnNames)
    {
        _columnNames = columnNames;
        return this;
    }

    public TempTableBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
    {
        _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public TempTableBuilder<T> WithDbColumnMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public TempTableBuilder<T> WithDbColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public TempTableBuilder<T> WithValueConverters(IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        _valueConverters = valueConverters;
        return this;
    }

    public TempTableBuilder<T> ConfigureTempTableOptions(Action<TempTableOptions> configureOptions)
    {
        _options = new TempTableOptions();
        if (configureOptions != null)
        {
            configureOptions(_options);
        }
        return this;
    }

    private string GetTableName()
    {
        if (!string.IsNullOrWhiteSpace(_options.TableName))
        {
            return _options.TableName;
        }

        if (!string.IsNullOrWhiteSpace(_options.PrefixName))
        {
            return _options.PrefixName + "-" + Guid.NewGuid();
        }

        return Guid.NewGuid().ToString();
    }

    public string Execute()
    {
        var tempTableName = $"[#{GetTableName()}]";
        var dataTable = _data.ToDataTable(_columnNames, valueConverters: _valueConverters);
        var sqlCreateTempTable = dataTable.GenerateTableDefinition(tempTableName, _columnNameMappings, _columnTypeMappings);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTempTable}");

        _connection.EnsureOpen();
        using (var createTempTableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTempTable))
        {
            createTempTableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {tempTableName}");

        dataTable.SqlBulkCopy(tempTableName, _columnNameMappings, _connection, _transaction);

        Log("End executing SqlBulkCopy.");

        return tempTableName;
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [TempTable]: {message}");
    }

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var tempTableName = $"[#{GetTableName()}]";
        var dataTable = await _data.ToDataTableAsync(_columnNames, valueConverters: _valueConverters, cancellationToken: cancellationToken);
        var sqlCreateTempTable = dataTable.GenerateTableDefinition(tempTableName, _columnNameMappings, _columnTypeMappings);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTempTable}");

        _connection.EnsureOpen();
        using (var createTempTableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTempTable))
        {
            await createTempTableCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {tempTableName}");

        await dataTable.SqlBulkCopyAsync(tempTableName, _columnNameMappings, _connection, _transaction, cancellationToken: cancellationToken);

        Log("End executing SqlBulkCopy.");

        return tempTableName;
    }
}
