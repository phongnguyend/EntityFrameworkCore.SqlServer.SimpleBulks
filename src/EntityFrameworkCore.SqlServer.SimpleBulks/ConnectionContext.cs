using Microsoft.Data.SqlClient;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public record struct ConnectionContext(SqlConnection Connection, SqlTransaction Transaction);