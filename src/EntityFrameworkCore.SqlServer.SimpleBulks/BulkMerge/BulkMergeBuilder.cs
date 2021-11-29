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
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public BulkMergeBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public BulkMergeBuilder(SqlTransaction transaction)
        {
            _transaction = transaction;
            _connection = transaction.Connection;
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

        public void Execute()
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNames = _updateColumnNames.Select(RemoveOperator).ToList();
            propertyNames.AddRange(_idColumns);
            propertyNames.AddRange(_insertColumnNames);
            propertyNames = propertyNames.Distinct().ToList();

            var dataTable = _data.ToDataTable(propertyNames);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, _idColumns);

            var mergeStatementBuilder = new StringBuilder();

            var joinCondition = string.Join(" and ", _idColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {Constants.Collation}" : string.Empty;
                return $"s.[{x}]{collation} = t.[{x}]{collation}";
            }));

            mergeStatementBuilder.AppendLine($"MERGE {_tableName} t");
            mergeStatementBuilder.AppendLine($"    USING [{temptableName}] s");
            mergeStatementBuilder.AppendLine($"ON ({joinCondition})");
            mergeStatementBuilder.AppendLine($"WHEN MATCHED");
            mergeStatementBuilder.AppendLine($"    THEN UPDATE SET");
            mergeStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _updateColumnNames.Select(x => "         " + CreateSetStatement(x, "t", "s"))));
            mergeStatementBuilder.AppendLine($"WHEN NOT MATCHED BY TARGET");
            mergeStatementBuilder.AppendLine($"    THEN INSERT ({string.Join(", ", _insertColumnNames)})");
            mergeStatementBuilder.AppendLine($"         VALUES ({string.Join(", ", _insertColumnNames.Select(x => $"s.{x}"))})");
            mergeStatementBuilder.AppendLine(";");

            _connection.EnsureOpen();

            using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, _connection, _transaction);

            using (var updateCommand = _connection.CreateTextCommand(_transaction, mergeStatementBuilder.ToString()))
            {
                var affectedRows = updateCommand.ExecuteNonQuery();
            }
        }

        private static string CreateSetStatement(string prop, string leftTable, string rightTable)
        {
            string sqlOperator = "=";
            string sqlProp = RemoveOperator(prop);

            if (prop.EndsWith("+="))
            {
                sqlOperator = "+=";
            }

            return $"{leftTable}.[{sqlProp}] {sqlOperator} {rightTable}.[{sqlProp}]";
        }

        private static string RemoveOperator(string prop)
        {
            var rs = prop.Replace("+=", "");
            return rs;
        }
    }
}
