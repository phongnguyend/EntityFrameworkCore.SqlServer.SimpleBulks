using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
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
    private BulkInsertOptions _options = BulkInsertOptions.DefaultOptions;
    private readonly ConnectionContext _connectionContext;

    public BulkInsertBuilder(ConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public BulkInsertBuilder<T> ToTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkInsertBuilder<T> WithOutputId(string idColumn)
    {
        _outputIdColumn = idColumn;
        return this;
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

    public BulkInsertBuilder<T> WithBulkOptions(BulkInsertOptions options)
    {
        _options = options ?? BulkInsertOptions.DefaultOptions;
        return this;
    }

    private string GetDbColumnName(string columnName)
    {
        if (_table.ColumnNameMappings == null)
        {
            return columnName;
        }

        return _table.ColumnNameMappings.TryGetValue(columnName, out string value) ? value : columnName;
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
            dataTable = data.ToDataTable(_columnNames, valueConverters: _table.ValueConverters);

            _connectionContext.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            _connectionContext.SqlBulkCopy(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options);
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

            dataTable = data.ToDataTable(columnsToInsert, valueConverters: _table.ValueConverters);

            _connectionContext.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            _connectionContext.SqlBulkCopy(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options);
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

            dataTable = data.ToDataTable(columnsToInsert, valueConverters: _table.ValueConverters);

            _connectionContext.EnsureOpen();

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            _connectionContext.SqlBulkCopy(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var temptableName = $"[#{Guid.NewGuid()}]";
        dataTable = data.ToDataTable(_columnNames, valueConverters: _table.ValueConverters, addIndexNumberColumn: true);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

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

        var returnedIds = new Dictionary<long, object>();

        var sqlMergeStatement = mergeStatementBuilder.ToString();

        Log($"Begin merging temp table:{Environment.NewLine}{sqlMergeStatement}");
        using (var updateCommand = _connectionContext.CreateTextCommand(sqlMergeStatement, _options))
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

        using var insertCommand = _connectionContext.CreateTextCommand(insertStatement, _options);

        _table.CreateSqlParameters(insertCommand, dataToInsert, columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        _connectionContext.EnsureOpen();

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
            dataTable = await data.ToDataTableAsync(_columnNames, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

            await _connectionContext.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await _connectionContext.SqlBulkCopyAsync(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options, cancellationToken);
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

            dataTable = await data.ToDataTableAsync(columnsToInsert, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

            await _connectionContext.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await _connectionContext.SqlBulkCopyAsync(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options, cancellationToken);
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

            dataTable = await data.ToDataTableAsync(columnsToInsert, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);

            await _connectionContext.EnsureOpenAsync(cancellationToken);

            Log($"Begin executing SqlBulkCopy. TableName: {_table.SchemaQualifiedTableName}");
            await _connectionContext.SqlBulkCopyAsync(dataTable, _table.SchemaQualifiedTableName, _table.ColumnNameMappings, _options, cancellationToken);
            Log("End executing SqlBulkCopy.");
            return;
        }

        var temptableName = $"[#{Guid.NewGuid()}]";
        dataTable = await data.ToDataTableAsync(_columnNames, valueConverters: _table.ValueConverters, addIndexNumberColumn: true, cancellationToken: cancellationToken);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

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

        var returnedIds = new Dictionary<long, object>();

        var sqlMergeStatement = mergeStatementBuilder.ToString();

        Log($"Begin merging temp table:{Environment.NewLine}{sqlMergeStatement}");
        using (var updateCommand = _connectionContext.CreateTextCommand(sqlMergeStatement, _options))
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

        using var insertCommand = _connectionContext.CreateTextCommand(insertStatement, _options);
        _table.CreateSqlParameters(insertCommand, dataToInsert, columnsToInsert).ForEach(x => insertCommand.Parameters.Add(x));

        Log($"Begin inserting: {Environment.NewLine}{insertStatement}");

        await _connectionContext.EnsureOpenAsync(cancellationToken);

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
