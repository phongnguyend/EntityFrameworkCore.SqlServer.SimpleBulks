using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate
{
    public class BulkUpdateBuilder<T>
    {
        private IEnumerable<T> _data;
        private string _tableName;
        private IEnumerable<string> _idColumns;
        private IEnumerable<string> _columnNames;
        private readonly SqlConnection _connection;

        public BulkUpdateBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public BulkUpdateBuilder<T> WithData(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        public BulkUpdateBuilder<T> ToTable(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public BulkUpdateBuilder<T> WithId(string idColumn)
        {
            _idColumns = new[] { idColumn };
            return this;
        }

        public BulkUpdateBuilder<T> WithId(IEnumerable<string> idColumns)
        {
            _idColumns = idColumns;
            return this;
        }

        public BulkUpdateBuilder<T> WithId(Expression<Func<T, object>> idSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
            return this;
        }

        public BulkUpdateBuilder<T> WithColumns(IEnumerable<string> columnNames)
        {
            _columnNames = columnNames;
            return this;
        }

        public BulkUpdateBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
        {
            _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            return this;
        }

        public void Execute()
        {
            var temptableName = "#" + Guid.NewGuid();

            var propertyNamesIncludeId = _columnNames.Select(RemoveOperator).ToList();
            propertyNamesIncludeId.AddRange(_idColumns);

            var dataTable = _data.ToDataTable(propertyNamesIncludeId);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, _idColumns);

            var joinCondition = string.Join(" and ", _idColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {Constants.Collation}" : string.Empty;
                return $"a.[{x}]{collation} = b.[{x}]{collation}";
            }));

            var updateStatementBuilder = new StringBuilder();
            updateStatementBuilder.AppendLine("update a set");
            updateStatementBuilder.AppendLine(string.Join("," + Environment.NewLine, _columnNames.Select(x => CreateSetStatement(x, "a", "b"))));
            updateStatementBuilder.AppendLine($"from {_tableName } a join [{ temptableName}] b on " + joinCondition);

            _connection.EnsureOpen();

            using (var createTemptableCommand = _connection.CreateCommand())
            {
                createTemptableCommand.CommandText = sqlCreateTemptable;
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, _connection);

            using (var updateCommand = _connection.CreateCommand())
            {
                updateCommand.CommandText = updateStatementBuilder.ToString();
                var affectedRows = updateCommand.ExecuteNonQuery();
            }

            _connection.EnsureClosed();
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
