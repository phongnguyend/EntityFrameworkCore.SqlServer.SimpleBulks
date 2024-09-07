using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.CustomSchema;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database
{
    [Table("Customers", Schema = TestConstants.Schema)]
    public class Customer
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CurrentCountryIsoCode { get; set; }

        public int Index { get; set; }

        public ICollection<Contact> Contacts { get; set; }
    }
}
