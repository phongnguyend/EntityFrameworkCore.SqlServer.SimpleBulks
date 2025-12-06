namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

public class ComplexOwnedTypeOrder
{
    public int Id { get; set; }

    public ComplexTypeAddress ComplexShippingAddress { get; set; }

    public OwnedTypeAddress OwnedShippingAddress { get; set; }
}