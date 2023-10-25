using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkSelect
{
    public class BulkSelectBuilder<T>
    {
        private string _tableName;
        private IEnumerable<string> _matchedColumns;
        private IEnumerable<string> _columnNames;
        private IDictionary<string, string> _dbColumnMappings;
        private BulkSelectOptions _options;
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public BulkSelectBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public BulkSelectBuilder(SqlConnection connection, SqlTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public BulkSelectBuilder<T> FromTable(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public BulkSelectBuilder<T> WithMatchedColumns(string matchedColumn)
        {
            _matchedColumns = new List<string> { matchedColumn };
            return this;
        }

        public BulkSelectBuilder<T> WithMatchedColumns(IEnumerable<string> matchedColumns)
        {
            _matchedColumns = matchedColumns;
            return this;
        }

        public BulkSelectBuilder<T> WithMatchedColumns(Expression<Func<T, object>> matchedColumnsSelector)
        {
            var matchedColumn = matchedColumnsSelector.Body.GetMemberName();
            _matchedColumns = string.IsNullOrEmpty(matchedColumn) ? matchedColumnsSelector.Body.GetMemberNames() : new List<string> { matchedColumn };
            return this;
        }

        public BulkSelectBuilder<T> WithColumns(IEnumerable<string> columnNames)
        {
            _columnNames = columnNames;
            return this;
        }

        public BulkSelectBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
        {
            _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            return this;
        }

        public BulkSelectBuilder<T> WithDbColumnMappings(IDictionary<string, string> dbColumnMappings)
        {
            _dbColumnMappings = dbColumnMappings;
            return this;
        }

        public BulkSelectBuilder<T> ConfigureBulkOptions(Action<BulkSelectOptions> configureOptions)
        {
            _options = new BulkSelectOptions();
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

        public IEnumerable<T> Execute(IEnumerable<T> machedValues)
        {
            var temptableName = "#" + Guid.NewGuid();

            var dataTable = machedValues.ToDataTable(_matchedColumns);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName);

            var joinCondition = string.Join(" and ", _matchedColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {Constants.Collation}" : string.Empty;
                return $"a.[{GetDbColumnName(x)}]{collation} = b.[{x}]{collation}";
            }));

            var selectQueryBuilder = new StringBuilder();
            selectQueryBuilder.AppendLine($"select {string.Join(", ", _columnNames.Select(x => CreateSelectStatement(x)))} ");
            selectQueryBuilder.AppendLine($"from {_tableName} a join [{temptableName}] b on " + joinCondition);

            _connection.EnsureOpen();

            using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, null, _connection, _transaction, _options);

            var results = new List<T>();

            var properties = typeof(T).GetProperties().Where(prop => _columnNames.Contains(prop.Name)).ToList();

            using var updateCommand = _connection.CreateTextCommand(_transaction, selectQueryBuilder.ToString());
            using var reader = updateCommand.ExecuteReader();
            while (reader.Read())
            {
                T obj = (T)Activator.CreateInstance(typeof(T));

                foreach (var prop in properties)
                {
                    if (!Equals(reader[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, reader[prop.Name], null);
                    }
                }

                results.Add(obj);

            }

            return results;
        }

        private string CreateSelectStatement(string colunmName)
        {
            return $"a.[{GetDbColumnName(colunmName)}] as [{colunmName}]";
        }
    }
}
