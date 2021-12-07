using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete
{
    public class BulkDeleteBuilder<T>
    {
        private IEnumerable<T> _data;
        private string _tableName;
        private IEnumerable<string> _idColumns;
        private BulkOptions _options;
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public BulkDeleteBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public BulkDeleteBuilder(SqlTransaction transaction)
        {
            _transaction = transaction;
            _connection = transaction.Connection;
        }

        public BulkDeleteBuilder(SqlConnection connection, SqlTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public BulkDeleteBuilder<T> WithData(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        public BulkDeleteBuilder<T> ToTable(string tableName)
        {
            _tableName = tableName;
            return this;
        }

        public BulkDeleteBuilder<T> WithId(string idColumn)
        {
            _idColumns = new[] { idColumn };
            return this;
        }

        public BulkDeleteBuilder<T> WithId(IEnumerable<string> idColumns)
        {
            _idColumns = idColumns;
            return this;
        }

        public BulkDeleteBuilder<T> WithId(Expression<Func<T, object>> idSelector)
        {
            var idColumn = idSelector.Body.GetMemberName();
            _idColumns = string.IsNullOrEmpty(idColumn) ? idSelector.Body.GetMemberNames() : new List<string> { idColumn };
            return this;
        }

        public BulkDeleteBuilder<T> ConfigureBulkOptions(Action<BulkOptions> configureOptions)
        {
            _options = new BulkOptions();
            configureOptions(_options);
            return this;
        }

        public void Execute()
        {
            var temptableName = "#" + Guid.NewGuid();
            var dataTable = _data.ToDataTable(_idColumns);
            var sqlCreateTemptable = dataTable.GenerateTableDefinition(temptableName, _idColumns);

            var joinCondition = string.Join(" and ", _idColumns.Select(x =>
            {
                string collation = dataTable.Columns[x].DataType == typeof(string) ?
                $" collate {Constants.Collation}" : string.Empty;
                return $"a.[{x}]{collation} = b.[{x}]{collation}";
            }));

            var deleteStatement = $"delete a from {_tableName} a join [{temptableName}] b on " + joinCondition;

            _connection.EnsureOpen();

            using (var createTemptableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTemptable))
            {
                createTemptableCommand.ExecuteNonQuery();
            }

            dataTable.SqlBulkCopy(temptableName, _connection, _transaction, _options);

            using (var deleteCommand = _connection.CreateTextCommand(_transaction, deleteStatement))
            {
                var affectedRows = deleteCommand.ExecuteNonQuery();
            }
        }

    }
}
