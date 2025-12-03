namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.PropertiesCache;

public class Order
{
    public int Id { get; set; }

    public Address ShippingAddress { get; set; }
}

public class Address
{
    public string Street { get; set; }

    public Location Location { get; set; }
}

public class Location
{
    public double Lat { get; set; }

    public double Lng { get; set; }
}