using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;

[ComplexType]
public class ComplexTypeAddress
{
    public string Street { get; set; }

    public ComplexTypeLocation Location { get; set; }
}

[ComplexType]
public class ComplexTypeLocation
{
    public double Lat { get; set; }

    public double Lng { get; set; }
}

public class JsonComplexTypeAddress
{
    public string Street { get; set; }

    public ComplexTypeLocation Location { get; set; }
}
