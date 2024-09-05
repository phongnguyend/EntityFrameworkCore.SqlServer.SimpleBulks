using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable
{
    public class TempTableBuilder<T>
    {
        private IEnumerable<T> _data;
        private IEnumerable<string> _columnNames;
        private readonly SqlConnection _connection;
        private readonly SqlTransaction _transaction;

        public TempTableBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public TempTableBuilder(SqlConnection connection, SqlTransaction transaction)
        {
            _connection = connection;
            _transaction = transaction;
        }

        public TempTableBuilder<T> WithData(IEnumerable<T> data)
        {
            _data = data;
            return this;
        }

        public TempTableBuilder<T> WithColumns(IEnumerable<string> columnNames)
        {
            _columnNames = columnNames;
            return this;
        }

        public TempTableBuilder<T> WithColumns(Expression<Func<T, object>> columnNamesSelector)
        {
            _columnNames = columnNamesSelector.Body.GetMemberNames().ToArray();
            return this;
        }

        public string Execute()
        {
            var tempTableName = $"[#{Guid.NewGuid()}]";
            var dataTable = _data.ToDataTable(_columnNames);
            var sqlCreateTempTable = dataTable.GenerateTableDefinition(tempTableName);

            Log($"Begin creating temp table:{Environment.NewLine}{sqlCreateTempTable}");

            _connection.EnsureOpen();
            using (var createTempTableCommand = _connection.CreateTextCommand(_transaction, sqlCreateTempTable))
            {
                createTempTableCommand.ExecuteNonQuery();
            }

            Log("End creating temp table.");

            Log($"Begin executing SqlBulkCopy. TableName: {tempTableName}");

            dataTable.SqlBulkCopy(tempTableName, null, _connection, _transaction);

            Log("End executing SqlBulkCopy.");

            return tempTableName;
        }

        private void Log(string message)
        {
            // _options?.LogTo?.Invoke($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} [TempTable]: {message}");
        }
    }
}
