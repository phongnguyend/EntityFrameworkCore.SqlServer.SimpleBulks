using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public class BulkMatchBuilder<T>
{
    private TableInfor _table;
    private IEnumerable<string> _matchedColumns;
    private IEnumerable<string> _returnedColumns;
    private BulkMatchOptions _options = BulkMatchOptions.DefaultOptions;
    private readonly ConnectionContext _connectionContext;

    public BulkMatchBuilder(ConnectionContext connectionContext)
    {
        _connectionContext = connectionContext;
    }

    public BulkMatchBuilder<T> WithTable(TableInfor table)
    {
        _table = table;
        return this;
    }

    public BulkMatchBuilder<T> WithMatchedColumns(IEnumerable<string> matchedColumns)
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

    public BulkMatchBuilder<T> WithReturnedColumns(IEnumerable<string> returnedColumns)
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

    public List<T> Execute(IEnumerable<T> machedValues)
    {
        var temptableName = $"[#{Guid.NewGuid()}]";

        var dataTable = machedValues.ToDataTable(_matchedColumns, valueConverters: _table.ValueConverters);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

        var joinCondition = string.Join(" AND ", _matchedColumns.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" COLLATE {_options.Collation}" : string.Empty;
            return $"a.[{_table.GetDbColumnName(x)}]{collation} = b.[{x}]{collation}";
        }));

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
                var prop = PropertiesCache<T>.GetProperty(propName);

                if (!Equals(reader[prop.Name], DBNull.Value))
                {
                    prop.SetValue(obj, GetValue(prop, reader, _table.ValueConverters), null);
                }
            }

            results.Add(obj);
        }

        Log($"End matching.");

        return results;
    }

    private string CreateSelectStatement(string colunmName)
    {
        return $"a.[{_table.GetDbColumnName(colunmName)}] as [{colunmName}]";
    }

    private void Log(string message)
    {
        _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkMatch]: {message}");
    }

    public async Task<List<T>> ExecuteAsync(IEnumerable<T> machedValues, CancellationToken cancellationToken = default)
    {
        var temptableName = $"[#{Guid.NewGuid()}]";

        var dataTable = await machedValues.ToDataTableAsync(_matchedColumns, valueConverters: _table.ValueConverters, cancellationToken: cancellationToken);
        var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, null, _table.ColumnTypeMappings);

        var joinCondition = string.Join(" AND ", _matchedColumns.Select(x =>
        {
            string collation = !string.IsNullOrEmpty(_options.Collation) && dataTable.Columns[x].DataType == typeof(string) ?
            $" COLLATE {_options.Collation}" : string.Empty;
            return $"a.[{_table.GetDbColumnName(x)}]{collation} = b.[{x}]{collation}";
        }));

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
                var prop = PropertiesCache<T>.GetProperty(propName);

                if (!Equals(reader[prop.Name], DBNull.Value))
                {
                    prop.SetValue(obj, GetValue(prop, reader, _table.ValueConverters), null);
                }
            }

            results.Add(obj);
        }

        Log($"End matching.");

        return results;
    }

    private static object GetValue(PropertyInfo property, SqlDataReader reader, IReadOnlyDictionary<string, ValueConverter> valueConverters)
    {
        if (valueConverters != null && valueConverters.TryGetValue(property.Name, out var converter))
        {
            return converter.ConvertFromProvider(reader[property.Name]);
        }

        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        var tempValue = reader[property.Name];
        var value = type.IsEnum && tempValue != null ? Enum.Parse(type, tempValue.ToString()) : tempValue;

        return value;
    }
}
