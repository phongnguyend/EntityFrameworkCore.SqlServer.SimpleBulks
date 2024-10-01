using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public class TempTableBuilder<T>
{
    private IEnumerable<T> _data;
    private IEnumerable<string> _columnNames;
    private IDictionary<string, string> _dbColumnMappings;
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

    public TempTableBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
    {
        _dbColumnMappings = dbColumnMappings;
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
        var dataTable = _data.ToDataTable(_columnNames);
        var sqlCreateTempTable = dataTable.GenerateTableDefinition(tempTableName, _dbColumnMappings);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTempTable}");

        _connection.EnsureOpen();
        using (var createTempTableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTempTable))
        {
            createTempTableCommand.ExecuteNonQuery();
        }

        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {tempTableName}");

        dataTable.SqlBulkCopy(tempTableName, _dbColumnMappings, _connection, _transaction);

        Log("End executing SqlBulkCopy.");

        return tempTableName;
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [TempTable]: {message}");
    }
}
