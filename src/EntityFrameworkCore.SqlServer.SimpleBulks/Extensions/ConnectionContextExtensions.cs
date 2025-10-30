using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class ConnectionContextExtensions
{
    private static readonly BulkOptions DefaultBulkOptions = new BulkOptions()
    {
        BatchSize = 0,
        Timeout = 30,
    };

    public static void EnsureOpen(this ConnectionContext connection)
    {
        var connectionState = connection.Connection.State;

        if (connectionState != ConnectionState.Open)
        {
            connection.Connection.Open();
        }
    }

    public static async Task EnsureOpenAsync(this ConnectionContext connection, CancellationToken cancellationToken = default)
    {
        var connectionState = connection.Connection.State;

        if (connectionState != ConnectionState.Open)
        {
            await connection.Connection.OpenAsync(cancellationToken);
        }
    }

    public static void EnsureClosed(this ConnectionContext connection)
    {
        var connectionState = connection.Connection.State;

        if (connectionState != ConnectionState.Closed)
        {
            connection.Connection.Close();
        }
    }

    public static SqlCommand CreateTextCommand(this ConnectionContext connection, string commandText, BulkOptions options = null)
    {
        options ??= DefaultBulkOptions;

        var command = connection.Connection.CreateCommand();
        command.Transaction = connection.Transaction;
        command.CommandText = commandText;
        command.CommandTimeout = options.Timeout;
        return command;
    }
}
