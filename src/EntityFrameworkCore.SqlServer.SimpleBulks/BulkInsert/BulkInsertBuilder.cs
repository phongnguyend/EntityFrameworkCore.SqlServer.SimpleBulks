using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public class BulkInsertBuilder<T>
{
    private TableInfor _table;
    private string _outputIdColumn;
    private OutputIdMode _outputIdMode = OutputIdMode.ServerGenerated;
    private IEnumerable<string> _columnNames;
    private IReadOnlyDictionary<string, string> _columnNameMappings;
    private IReadOnlyDictionary<string, string> _columnTypeMappings;
    private BulkInsertOptions _options;
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    public BulkInsertBuilder(SqlConnection connection)
    {
        _connection = connection;
    }

    public BulkInsertBuilder(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public BulkInsertBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    [Obsolete("Typo Issue, Shoud use WithOutputId")]
    public BulkInsertBuilder<T> WithOuputId(string idColumn)
    {
        return WithOutputId(idColumn);
    }

    public BulkInsertBuilder<T> WithOutputId(string idColumn)
    {
        _outputIdColumn = idColumn;
        return this;
    }

    [Obsolete("Typo Issue, Shoud use WithOutputId")]
    public BulkInsertBuilder<T> WithOuputId(Expression<Func<T, object>> idSelector)
    {
        return WithOutputId(idSelector);
    }

    public BulkInsertBuilder<T> WithOutputId(Expression<Func<T, object>> idSelector)
    {
        _outputIdColumn = idSelector.Body.GetMemberName();
        return this;
    }

    public BulkInsertBuilder<T> WithOutputIdMode(OutputIdMode outputIdMode)
    {
        _outputIdMode = outputIdMode;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(IEnumerable<string> columnNames)
    {
        _columnNames = columnNames;
        return this;
    }

    public BulkInsertBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
    {
        _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkInsertBuilder<T> WithDbColumnMappings(IReadOnlyDictionary<string, string> columnNameMappings)
    {
        _columnNameMappings = columnNameMappings;
        return this;
    }

    public BulkInsertBuilder<T> WithDbColumnTypeMappings(IReadOnlyDictionary<string, string> columnTypeMappings)
    {
        _columnTypeMappings = columnTypeMappings;
        return this;
    }

    public BulkInsertBuilder<T> ConfigureBulkOptions(Action<BulkInsertOptions> configureOptions)
    {
        _options = new BulkInsertOptions();
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

    private bool ReturnGeneratedId => !string.IsNullOrWhiteSpace(_outputIdColumn);

    private PropertyInfo GetIdProperty()
    {
        return typeof(T).GetProperty(_outputIdColumn);
    }

    private static Action<T, Guid> GetSetIdMethod(PropertyInfo idProperty)
    {
        return (Action<T, Guid>)Delegate.CreateDelegate(typeof(Action<T, Guid>), idProperty.GetSetMethod());
    }

    public void Execute(IEnumerable<T> data)
    {
        if (data.Count() == 1)
        {
            SingleInsert(data.First());
            return;
        }

        DataTable dataTable;
        if (!ReturnGeneratedId)
        {
            dataTable = data.ToDataTable(_columnNames);

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _columnNameMappings, _connection, _transaction, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_options.KeepIdentity)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            dataTable = data.ToDataTable(columnsToInsert);

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _columnNameMappings, _connection, _transaction, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var idProperty = GetIdProperty();

        if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var setId = GetSetIdMethod(idProperty);

            foreach (var row in data)
            {
                setId(row, SequentialGuidGenerator.Next());
            }

            dataTable = data.ToDataTable(columnsToInsert);

            _connection.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            dataTable.SqlBulkCopy(_table.SchemaQualifiedTableName, _columnNameMappings, _connection, _transaction, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var temptableName = $"[#{Guid.NewGuid()}]";
        dataTable = data.ToDataTable(_columnNames, addIndexNumberColumn: true);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _columnTypeMappings);

        var mergeStatementBuilder = new StringBuilder();
        mergeStatementBuilder.AppendLine($"MERGE INTO {_table.SchemaQualifiedTableName}");
        mergeStatementBuilder.AppendLine("USING");
        mergeStatementBuilder.AppendLine("(");
        mergeStatementBuilder.AppendLine($"SELECT TOP {data.Count()} * FROM {temptableName} ORDER BY [{Constants.AutoGeneratedIndexNumberColumn}]");
        mergeStatementBuilder.AppendLine(") AS Src");
        mergeStatementBuilder.AppendLine("ON 1 = 0");
        mergeStatementBuilder.AppendLine("WHEN NOT MATCHED BY TARGET THEN");
        mergeStatementBuilder.AppendLine($"INSERT ({string.Join(", ", _columnNames.Select(x => $"[{GetDbColumnName(x)}]"))})");
        mergeStatementBuilder.AppendLine($"VALUES ({string.Join(", ", _columnNames.Select(x => $"Src.[{x}]"))})");
        mergeStatementBuilder.AppendLine($"OUTPUT inserted.[{GetDbColumnName(_outputIdColumn)}], Src.[{Constants.AutoGeneratedIndexNumberColumn}]");
        mergeStatementBuilder.AppendLine(";");

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

        var returnedIds = new Dictionary<long, object>();

        var sqlMergeStatement = mergeStatementBuilder.ToString();

        Log($"Begin merging temp table:{Environment.NewLine}{sqlMergeStatement}");
        using (var updateCommand = _connection.CreateTextCommand(_transaction, sqlMergeStatement, _options))
        {
            using var reader = updateCommand.ExecuteReader();
            var dbColumn = GetDbColumnName(_outputIdColumn);
            while (reader.Read())
            {
                returnedIds[(reader[Constants.AutoGeneratedIndexNumberColumn] as long?).Value] = reader[dbColumn];
            }
        }
        Log("End merging temp table.");

        long idx = 0;
        foreach (var row in data)
        {
            idProperty.SetValue(row, returnedIds[idx]);
            idx++;
        }
    }

    public void SingleInsert(T dataToInsert)
    {
        var insertStatementBuilder = new StringBuilder();

        var columnsToInsert = _columnNames.Select(x => x).ToList();

        if (_options.KeepIdentity)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"[{GetDbColumnName(x)}]"))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else if (ReturnGeneratedId && _outputIdMode == OutputIdMode.ClientGenerated)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);
            setId(dataToInsert, SequentialGuidGenerator.Next());

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"[{GetDbColumnName(x)}]"))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else
        {
            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"[{GetDbColumnName(x)}]"))})");

            if (ReturnGeneratedId)
            {
                insertStatementBuilder.AppendLine($"OUTPUT inserted.[{GetDbColumnName(_outputIdColumn)}]");
            }

            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }

        var insertStatement = insertStatementBuilder.ToString();

        using var insertCommand = _connection.CreateTextCommand(_transaction, insertStatement, _options);
        dataToInsert.ToSqlParameters(columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        _connection.EnsureOpen();

        if (_options.KeepIdentity || !ReturnGeneratedId)
        {
            var affectedRow = insertCommand.ExecuteNonQuery();
        }
        else
        {
            var dbColumn = GetDbColumnName(_outputIdColumn);
            var idProperty = GetIdProperty();

            using var reader = insertCommand.ExecuteReader();
            while (reader.Read())
            {
                var returnedId = reader[dbColumn];

                idProperty.SetValue(dataToInsert, returnedId);
                break;
            }
        }

        Log($"End inserting.");
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkInsert]: {message}");
    }

    public async Task ExecuteAsync(IEnumerable<T> data, CancellationToken cancellationToken = default)
    {
        if (data.Count() == 1)
        {
            await SingleInsertAsync(data.First(), cancellationToken);
            return;
        }

        DataTable dataTable;
        if (!ReturnGeneratedId)
        {
            dataTable = await data.ToDataTableAsync(_columnNames, cancellationToken: cancellationToken);

            await _connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await dataTable.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _columnNameMappings, _connection, _transaction, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        if (_options.KeepIdentity)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            dataTable = await data.ToDataTableAsync(columnsToInsert, cancellationToken: cancellationToken);

            await _connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await dataTable.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _columnNameMappings, _connection, _transaction, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var idProperty = GetIdProperty();

        if (_outputIdMode == OutputIdMode.ClientGenerated)
        {
            var columnsToInsert = _columnNames.Select(x => x).ToList();
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var setId = GetSetIdMethod(idProperty);

            foreach (var row in data)
            {
                setId(row, SequentialGuidGenerator.Next());
            }

            dataTable = await data.ToDataTableAsync(columnsToInsert, cancellationToken: cancellationToken);

            await _connection.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await dataTable.SqlBulkCopyAsync(_table.SchemaQualifiedTableName, _columnNameMappings, _connection, _transaction, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var temptableName = $"[#{Guid.NewGuid()}]";
        dataTable = await data.ToDataTableAsync(_columnNames, addIndexNumberColumn: true, cancellationToken: cancellationToken);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _columnTypeMappings);

        var mergeStatementBuilder = new StringBuilder();
        mergeStatementBuilder.AppendLine($"MERGE INTO {_table.SchemaQualifiedTableName}");
        mergeStatementBuilder.AppendLine("USING");
        mergeStatementBuilder.AppendLine("(");
        mergeStatementBuilder.AppendLine($"SELECT TOP {data.Count()} * FROM {temptableName} ORDER BY [{Constants.AutoGeneratedIndexNumberColumn}]");
        mergeStatementBuilder.AppendLine(") AS Src");
        mergeStatementBuilder.AppendLine("ON 1 = 0");
        mergeStatementBuilder.AppendLine("WHEN NOT MATCHED BY TARGET THEN");
        mergeStatementBuilder.AppendLine($"INSERT ({string.Join(", ", _columnNames.Select(x => $"[{GetDbColumnName(x)}]"))})");
        mergeStatementBuilder.AppendLine($"VALUES ({string.Join(", ", _columnNames.Select(x => $"Src.[{x}]"))})");
        mergeStatementBuilder.AppendLine($"OUTPUT inserted.[{GetDbColumnName(_outputIdColumn)}], Src.[{Constants.AutoGeneratedIndexNumberColumn}]");
        mergeStatementBuilder.AppendLine(";");

        await _connection.EnsureOpenAsync(cancellationToken);

        Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTemptable}");
        using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable, _options))
        {
            await createTemptableCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        Log("End creating temp table.");

        Log($"Begin executing SqlBulkCopy. TableName: {temptableName}");
        await dataTable.SqlBulkCopyAsync(temptableName, null, _connection, _transaction, _options, cancellationToken);
        Log("End executing SqlBulkCopy.");

        var returnedIds = new Dictionary<long, object>();

        var sqlMergeStatement = mergeStatementBuilder.ToString();

        Log($"Begin merging temp table:{Environment.NewLine}{sqlMergeStatement}");
        using (var updateCommand = _connection.CreateTextCommand(_transaction, sqlMergeStatement, _options))
        {
            using var reader = await updateCommand.ExecuteReaderAsync(cancellationToken);
            var dbColumn = GetDbColumnName(_outputIdColumn);
            while (await reader.ReadAsync(cancellationToken))
            {
                returnedIds[(reader[Constants.AutoGeneratedIndexNumberColumn] as long?).Value] = reader[dbColumn];
            }
        }
        Log("End merging temp table.");

        long idx = 0;
        foreach (var row in data)
        {
            idProperty.SetValue(row, returnedIds[idx]);
            idx++;
        }
    }

    public async Task SingleInsertAsync(T dataToInsert, CancellationToken cancellationToken = default)
    {
        var insertStatementBuilder = new StringBuilder();

        var columnsToInsert = _columnNames.Select(x => x).ToList();

        if (_options.KeepIdentity)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"[{GetDbColumnName(x)}]"))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else if (ReturnGeneratedId && _outputIdMode == OutputIdMode.ClientGenerated)
        {
            if (!columnsToInsert.Contains(_outputIdColumn))
            {
                columnsToInsert.Add(_outputIdColumn);
            }

            var idProperty = GetIdProperty();
            var setId = GetSetIdMethod(idProperty);
            setId(dataToInsert, SequentialGuidGenerator.Next());

            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"[{GetDbColumnName(x)}]"))})");
            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }
        else
        {
            insertStatementBuilder.AppendLine($"INSERT INTO {_table.SchemaQualifiedTableName} ({string.Join(", ", columnsToInsert.Select(x => $"[{GetDbColumnName(x)}]"))})");

            if (ReturnGeneratedId)
            {
                insertStatementBuilder.AppendLine($"OUTPUT inserted.[{GetDbColumnName(_outputIdColumn)}]");
            }

            insertStatementBuilder.AppendLine($"VALUES ({string.Join(", ", columnsToInsert.Select(x => $"@{x}"))})");
        }

        var insertStatement = insertStatementBuilder.ToString();

        using var insertCommand = _connection.CreateTextCommand(_transaction, insertStatement, _options);
        dataToInsert.ToSqlParameters(columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        await _connection.EnsureOpenAsync(cancellationToken);

        if (_options.KeepIdentity || !ReturnGeneratedId)
        {
            var affectedRow = await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            var dbColumn = GetDbColumnName(_outputIdColumn);
            var idProperty = GetIdProperty();

            using var reader = await insertCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var returnedId = reader[dbColumn];

                idProperty.SetValue(dataToInsert, returnedId);
                break;
            }
        }

        Log($"End inserting.");
    }
}
