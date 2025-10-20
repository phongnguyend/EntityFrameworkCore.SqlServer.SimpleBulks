using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class SqlConnectionExtensions
{
    public static void EnsureOpen(this SqlConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Open)
        {
            connection.Open();
        }
    }

    public static async Task EnsureOpenAsync(this SqlConnection connection, CancellationToken cancellationToken = default)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
    }

    public static void EnsureClosed(this SqlConnection connection)
    {
        var connectionState = connection.State;

        if (connectionState != ConnectionState.Closed)
        {
            connection.Close();
        }
    }

    public static SqlCommand CreateTextCommand(this SqlConnection connection, SqlTransaction transaction, string commandText, BulkOptions options = null)
    {
        options ??= new BulkOptions()
        {
            BatchSize = 0,
            Timeout = 30,
        };

        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        command.CommandTimeout = options.Timeout;
        return command;
    }
}
