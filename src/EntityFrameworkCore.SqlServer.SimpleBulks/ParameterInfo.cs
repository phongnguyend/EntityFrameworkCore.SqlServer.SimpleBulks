using Microsoft.Data.SqlClient;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class ParameterInfo
{
    public string Name { get; set; }

    public string Type { get; set; }

    public SqlParameter Parameter { get; set; }

    public bool FromConverter { get; set; }

    public override string ToString()
    {
        if (FromConverter)
        {
            return $"{Name} (Type: {Type}), (FromConverter: {FromConverter})";
        }

        return $"{Name} (Type: {Type})";
    }
}
