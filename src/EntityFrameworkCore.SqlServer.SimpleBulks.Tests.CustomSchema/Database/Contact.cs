using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.CustomSchema;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

[Table("Contacts", Schema = TestConstants.Schema)]
public class Contact
{
    public Guid Id { get; set; }

    public string EmailAddress { get; set; }

    public string PhoneNumber { get; set; }

    public string CountryIsoCode { get; set; }

    public int Index { get; set; }

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; }
}
