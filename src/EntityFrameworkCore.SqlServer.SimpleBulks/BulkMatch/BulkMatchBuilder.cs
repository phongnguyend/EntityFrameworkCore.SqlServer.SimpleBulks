using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public class BulkMatchBuilder<T>
{
    private TableInfor<T> _table;
    private IReadOnlyCollection<string> _matchedColumns;
    private IReadOnlyCollection<string> _returnedColumns;
    private BulkMatchOptions _options = BulkMatchOptions.DefaultOptions;
    private readonly ConnectionContext _connectionContext;

    public BulkMatchBuilder(ConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public BulkMatchBuilder<T> WithTable(TableInfor<T> table)
    {
        _table = table;
        return this;
    }

    public BulkMatchBuilder<T> WithMatchedColumns(IReadOnlyCollection<string> matchedColumns)
    {
        _matchedColumns = matchedColumns;
        return this;
    }

    public BulkMatchBuilder<T> WithMatchedColumns(Expression<Func<T, object>> matchedColumnsSelector)
    {
        var matchedColumn = matchedColumnsSelector.Body.GetMemberName();
        _matchedColumns = string.IsNullOrEmpty(matchedColumn) ? matchedColumnsSelector.Body.GetMemberNames() : new List<string> { matchedColumn };
        return this;
    }

    public BulkMatchBuilder<T> WithReturnedColumns(IReadOnlyCollection<string> returnedColumns)
    {
        _returnedColumns = returnedColumns;
        return this;
    }

    public BulkMatchBuilder<T> WithReturnedColumns(Expression<Func<T, object>> returnedColumnsSelector)
    {
        _returnedColumns = returnedColumnsSelector.Body.GetMemberNames().ToArray();
        return this;
    }

    public BulkMatchBuilder<T> WithBulkOptions(BulkMatchOptions options)
    {
        _options = options ?? BulkMatchOptions.DefaultOptions;
        return this;
    }

    private List<string> GetKeys()
    {
        var copiedPropertyNames = _matchedColumns.ToList();

        if (_table.Discriminator != null && !copiedPropertyNames.Contains(_table.Discriminator.PropertyName))
        {
            copiedPropertyNames.Add(_table.Discriminator.PropertyName);
        }

        return copiedPropertyNames;
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

    private string CreateSelectStatement(string colunmName)
    {
        return $"a.[{_table.GetDbColumnName(colunmName)}] as [{colunmName}]";
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkMatch]: {message}");
    }

    public List<T> Execute(IReadOnlyCollection<T> machedValues)
    {
        var temptableName = $"[#{Guid.NewGuid()}]";

        var dataTable = machedValues.ToDataTable(_matchedColumns, valueConverters: _table.ValueConverters, discriminator: _table.Discriminator);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

        var joinCondition = CreateJoinCondition(dataTable);

        var selectQueryBuilder = new StringBuilder();
        selectQueryBuilder.AppendLine($"SELECT {string.Join(", ", _returnedColumns.Select(x => CreateSelectStatement(x)))} ");
        selectQueryBuilder.AppendLine($"FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition);

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

        var selectQuery = selectQueryBuilder.ToString();

        Log($"Begin matching:{Environment.NewLine}{selectQuery}");

        var results = new List<T>();

        using var updateCommand = _connectionContext.CreateTextCommand(selectQuery, _options);
        using var reader = updateCommand.ExecuteReader();
        while (reader.Read())
        {
            T obj = (T)Activator.CreateInstance(typeof(T));

            foreach (var propName in _returnedColumns)
            {
                if (!Equals(reader[propName], DBNull.Value))
                {
                    PropertiesCache<T>.SetPropertyValue(propName, obj, reader[propName], _table.ValueConverters);
                }
            }

            results.Add(obj);
        }

        Log($"End matching.");

        return results;
    }

    public async Task<List<T>> ExecuteAsync(IReadOnlyCollection<T> machedValues, CancellationToken cancellationToken = default)
    {
        var temptableName = $"[#{Guid.NewGuid()}]";

        var dataTable = await machedValues.ToDataTableAsync(_matchedColumns, valueConverters: _table.ValueConverters, discriminator: _table.Discriminator, cancellationToken: cancellationToken);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

        var joinCondition = CreateJoinCondition(dataTable);

        var selectQueryBuilder = new StringBuilder();
        selectQueryBuilder.AppendLine($"SELECT {string.Join(", ", _returnedColumns.Select(x => CreateSelectStatement(x)))} ");
        selectQueryBuilder.AppendLine($"FROM {_table.SchemaQualifiedTableName} a JOIN {temptableName} b ON " + joinCondition);

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

        var selectQuery = selectQueryBuilder.ToString();

        Log($"Begin matching:{Environment.NewLine}{selectQuery}");

        var results = new List<T>();

        using var updateCommand = _connectionContext.CreateTextCommand(selectQuery, _options);
        using var reader = await updateCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            T obj = (T)Activator.CreateInstance(typeof(T));

            foreach (var propName in _returnedColumns)
            {
                if (!Equals(reader[propName], DBNull.Value))
                {
                    PropertiesCache<T>.SetPropertyValue(propName, obj, reader[propName], _table.ValueConverters);
                }
            }

            results.Add(obj);
        }

        Log($"End matching.");

        return results;
    }
}
