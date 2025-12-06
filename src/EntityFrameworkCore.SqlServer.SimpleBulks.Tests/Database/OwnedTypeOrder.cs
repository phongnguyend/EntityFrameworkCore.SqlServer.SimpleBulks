using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

public class OwnedTypeOrder
{
    public int Id { get; set; }

    public OwnedTypeAddress ShippingAddress { get; set; }
}

[Owned]
public class OwnedTypeAddress
{
    public string Street { get; set; }

    public OwnedTypeLocation Location { get; set; }
}

[Owned]
public class OwnedTypeLocation
{
    public double Lat { get; set; }

    public double Lng { get; set; }
}