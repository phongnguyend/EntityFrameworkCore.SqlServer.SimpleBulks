namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;

public class Contact
{
    public Guid Id { get; set; }

    public string EmailAddress { get; set; }

    public string PhoneNumber { get; set; }

    public string CountryIsoCode { get; set; }

    public int Index { get; set; }

    public Season? Season { get; set; }

    public Season? SeasonAsString { get; set; }

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; }
}
