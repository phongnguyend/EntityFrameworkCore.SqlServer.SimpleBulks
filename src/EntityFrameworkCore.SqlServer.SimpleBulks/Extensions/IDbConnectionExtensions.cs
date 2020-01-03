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
    }
}
