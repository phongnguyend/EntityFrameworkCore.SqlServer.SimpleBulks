namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database
{
    public class Customer
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Index { get; set; }

        public ICollection<Contact> Contacts { get; set; }
    }
}
