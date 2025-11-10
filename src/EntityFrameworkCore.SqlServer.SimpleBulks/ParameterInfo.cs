using Microsoft.Data.SqlClient;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class ParameterInfo
{
    public string Name { get; set; }

    public string Type { get; set; }

    public SqlParameter Parameter { get; set; }
}
