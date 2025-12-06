using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

public class ComplexTypeOrder
{
    public int Id { get; set; }

    public ComplexTypeAddress ShippingAddress { get; set; }
}

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