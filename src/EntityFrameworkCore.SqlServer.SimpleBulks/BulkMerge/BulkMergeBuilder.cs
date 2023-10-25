using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
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
        private IEnumerable<T> _data;
        private string _tableName;
        private IEnumerable<string> _idColumns;
        private IEnumerable<string> _updateColumnNames;
        private IEnumerable<string> _insertColumnNames;
        private IDictionary<string, string> _dbColumnMappings;
        private BulkMergeOptions _options;
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public BulkMergeBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public BulkMergeBuilder(SqlConnection connection, SqlTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public BulkMergeBuilder<T> WithData(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        public BulkMergeBuilder<T> ToTable(string tableName)
        {
            _tableName = tableName;
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

        public BulkMergeResult Execute()
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNames = _updateColumnNames.Select(RemoveOperator).ToList();
            propertyNames.AddRange(_idColumns);
            propertyNames.AddRange(_insertColumnNames);
            propertyNames = propertyNames.Distinct().ToList();

            var dataTable = _data.ToDataTable(propertyNames);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName);

            var mergeStatementBuilder = new StringBuilder();

            var joinCondition = string.Join(" and ", _idColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {Constants.Collation}" : string.Empty;
                return $"s.[{x}]{collation} = t.[{GetDbColumnName(x)}]{collation}";
            }));

            var hint = _options.WithHoldLock ? " WITH (HOLDLOCK)" : string.Empty;

            mergeStatementBuilder.AppendLine($"MERGE {_tableName}{hint} t");
            mergeStatementBuilder.AppendLine($"    USING [{temptableName}] s");
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

            mergeStatementBuilder.AppendLine(";");

            _connection.EnsureOpen();

            using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, null, _connection, _transaction, _options);

            using var updateCommand = _connection.CreateTextCommand(_transaction, mergeStatementBuilder.ToString());
            var affectedRows = updateCommand.ExecuteNonQuery();

            return new BulkMergeResult
            {
                AffectedRows = affectedRows
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

        private static string RemoveOperator(string prop)
        {
            var rs = prop.Replace("+=", "");
            return rs;
        }
    }
}
