using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;

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
