using Microsoft.Data.SqlClient;
using System.Data;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class IDbConnectionExtensions
    {
        public static SqlConnection AsSqlConnection(this IDbConnection connection)
        {
            return connection as SqlConnection;
        }

        public static void EnsureOpen(this IDbConnection connection)
        {
            var connectionState = connection.State;

            if (connectionState != ConnectionState.Open)
            {
                connection.Open();
            }
        }

        public static void EnsureClosed(this IDbConnection connection)
        {
            var connectionState = connection.State;

            if (connectionState != ConnectionState.Closed)
            {
                connection.Close();
            }
        }
    }
}
