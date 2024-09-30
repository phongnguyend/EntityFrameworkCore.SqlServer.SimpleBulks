﻿using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge
{
    public class BulkMergeBuilder<T>
    {
        private TableInfor _table;
        private IEnumerable<string> _idColumns;
        private IEnumerable<string> _updateColumnNames;
        private IEnumerable<string> _insertColumnNames;
        private IDictionary<string, string> _dbColumnMappings;
        private string _outputIdColumn;
        private BulkMergeOptions _options;
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public BulkMergeBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public BulkMergeBuilder(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public BulkMergeBuilder<T> ToTable(TableInfor table)
        {
            _table = table;
            return this;
        }

        public BulkMergeBuilder<T> WithId(string idColumn)
        {
            _idColumns = new[] { idColumn };
            return this;
        }

        public BulkMergeBuilder<T> WithId(IEnumerable<string> idColumns)
        {
            _idColumns = idColumns;
            return this;
        }

        public BulkMergeBuilder<T> WithId(Expression<Func<T, object>> idSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
            return this;
        }

        public BulkMergeBuilder<T> WithUpdateColumns(IEnumerable<string> updateColumnNames)
        {
            _updateColumnNames = updateColumnNames;
            return this;
        }

        public BulkMergeBuilder<T> WithUpdateColumns(Expression<Func<T, object>> updateColumnNamesSelector)
        {
            _updateColumnNames = updateColumnNamesSelector.Body.GetMemberNames().ToArray();
            return this;
        }

        public BulkMergeBuilder<T> WithInsertColumns(IEnumerable<string> insertColumnNames)
        {
            _insertColumnNames = insertColumnNames;
            return this;
        }

        public BulkMergeBuilder<T> WithInsertColumns(Expression<Func<T, object>> insertColumnNamesSelector)
        {
            _insertColumnNames = insertColumnNamesSelector.Body.GetMemberNames().ToArray();
            return this;
        }

        public BulkMergeBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
        {
            _dbColumnMappings = dbColumnMappings;
            return this;
        }

        public BulkMergeBuilder<T> WithOutputId(string idColumn)
        {
            _outputIdColumn = idColumn;
            return this;
        }

        public BulkMergeBuilder<T> WithOutputId(Expression<Func<T, object>> idSelector)
        {
            _outputIdColumn = idSelector.Body.GetMemberName();
            return this;
        }

        public BulkMergeBuilder<T> ConfigureBulkOptions(Action<BulkMergeOptions> configureOptions)
        {
            _options = new BulkMergeOptions();
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

            return _dbColumnMappings.ContainsKey(columnName) ? _dbColumnMappings[columnName] : columnName;
        }

        public BulkMergeResult Execute(IEnumerable<T> data)
        {
            if (!_updateColumnNames.Any() && !_insertColumnNames.Any())
            {
                return new BulkMergeResult();
            }

            bool returnDbGeneratedId = _options.ReturnDbGeneratedId && !string.IsNullOrEmpty(_outputIdColumn) && _insertColumnNames.Any();

            var temptableName = $"[#{Guid.NewGuid()}]";

            var propertyNames = _updateColumnNames.Select(RemoveOperator).ToList();
            propertyNames.AddRange(_idColumns);
            propertyNames.AddRange(_insertColumnNames);
            propertyNames = propertyNames.Distinct().ToList();

            var dataTable = data.ToDataTable(propertyNames, addIndexNumberColumn: returnDbGeneratedId);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName);

            var mergeStatementBuilder = new StringBuilder();

            var joinCondition = string.Join(" and ", _idColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {_options.Collation}" : string.Empty;
                return $"s.[{x}]{collation} = t.[{GetDbColumnName(x)}]{collation}";
            }));

            var hint = _options.WithHoldLock ? " WITH (HOLDLOCK)" : string.Empty;

            mergeStatementBuilder.AppendLine($"MERGE {_table.SchemaQualifiedTableName}{hint} t");
            mergeStatementBuilder.AppendLine($"    USING {temptableName} s");
            mergeStatementBuilder.AppendLine($"ON ({joinCondition})");

            if (_updateColumnNames.Any())
            {
                mergeStatementBuilder.AppendLine($"WHEN MATCHED");
                mergeStatementBuilder.AppendLine($"    THEN UPDATE SET");
                mergeStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _updateColumnNames.Select(x => "         " + CreateSetStatement(x, "t", "s"))));
            }

            if (_insertColumnNames.Any())
            {
                mergeStatementBuilder.AppendLine($"WHEN NOT MATCHED BY TARGET");
                mergeStatementBuilder.AppendLine($"    THEN INSERT ({string.Join(", ", _insertColumnNames.Select(x => $"[{GetDbColumnName(x)}]"))})");
                mergeStatementBuilder.AppendLine($"         VALUES ({string.Join(", ", _insertColumnNames.Select(x => $"s.[{x}]"))})");
            }

            if (returnDbGeneratedId)
            {
                mergeStatementBuilder.AppendLine($"OUTPUT $action, inserted.[{GetDbColumnName(_outputIdColumn)}], s.[{Constants.AutoGeneratedIndexNumberColumn}]");
            }
            else
            {
                mergeStatementBuilder.AppendLine($"OUTPUT $action");
            }

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

            var sqlMergeStatement = mergeStatementBuilder.ToString();

            Log($"Begin merging temp table:{Environment.NewLine}{sqlMergeStatement}");

            BulkMergeResult result = new();
            Dictionary<long, object> returnedIds = null;
            string outputIdDbColumnName = null;

            if (returnDbGeneratedId)
            {
                returnedIds = new Dictionary<long, object>();
                outputIdDbColumnName = GetDbColumnName(_outputIdColumn);
            }

            using (var updateCommand = _connection.CreateTextCommand(_transaction, sqlMergeStatement, _options))
            {
                using var reader = updateCommand.ExecuteReader();

                while (reader.Read())
                {
                    var action = reader["$action"] as string;

                    if (action == "INSERT")
                    {
                        if (returnDbGeneratedId)
                        {
                            returnedIds[(reader[Constants.AutoGeneratedIndexNumberColumn] as long?).Value] = reader[outputIdDbColumnName];
                        }

                        result.InsertedRows++;
                    }
                    else if (action == "UPDATE")
                    {
                        result.UpdatedRows++;
                    }

                    result.AffectedRows++;
                }
            }

            Log("End merging temp table.");

            if (returnDbGeneratedId)
            {
                var idProperty = typeof(T).GetProperty(_outputIdColumn);

                long idx = 0;
                foreach (var row in data)
                {
                    if (returnedIds.TryGetValue(idx, out object id))
                    {
                        idProperty.SetValue(row, id);
                    }

                    idx++;
                }
            }

            return result;
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

        private static string RemoveOperator(string prop)
        {
            var rs = prop.Replace("+=", "");
            return rs;
        }

        private void Log(string message)
        {
            _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [BulkMerge]: {message}");
        }
    }
}
