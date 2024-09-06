using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks
{
    [WarmupCount(0)]
    [IterationCount(1)]
    [InvocationCount(1)]
    [MemoryDiagnoser]
    public class BulkMatchMultipleColumnsBenchmarks
    {
        private TestDbContext _context;
        private List<Customer> _customers;
        private List<Contact> _contacts;
        private List<Contact> _contactsToMatch;

        [Params(100, 1000, 10_000, 100_000)]
        public int RowsCount { get; set; }

        //[Params(250_000, 500_000, 1_000_000)]
        //public int RowsCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _context = new TestDbContext($"Server=127.0.0.1;Database=EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#");
            _context.Database.EnsureCreated();
            _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

            var isoCodes = new string[] { "VN", "US", "GB" };
            var random = new Random(2024);

            _customers = new List<Customer>();

            for (int i = 0; i < RowsCount; i++)
            {
                var customer = new Customer
                {
                    FirstName = "FirstName " + i,
                    LastName = "LastName " + i,
                    Index = i,
                    CurrentCountryIsoCode = isoCodes[random.Next(isoCodes.Length)]
                };

                customer.Contacts = new List<Contact>();

                for (int j = 0; j < 5; j++)
                {
                    customer.Contacts.Add(new Contact
                    {
                        EmailAddress = $"EmailAddress {i} - {j}",
                        PhoneNumber = $"PhoneNumber {i} - {j}",
                        CountryIsoCode = isoCodes[random.Next(isoCodes.Length)],
                        Index = j,
                    });
                }

                _customers.Add(customer);
            }

            _context.BulkInsert(_customers,
                opt =>
                {
                    opt.Timeout = 0;
                });

            foreach (var customer in _customers)
            {
                foreach (var contact in customer.Contacts)
                {
                    contact.CustomerId = customer.Id;
                }
            }

            _contacts = _customers.SelectMany(x => x.Contacts).ToList();

            _context.BulkInsert(_contacts,
                opt =>
                {
                    opt.Timeout = 0;
                });

            _contactsToMatch = _customers.Select(x => new Contact { CustomerId = x.Id, CountryIsoCode = x.CurrentCountryIsoCode }).ToList();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _context.Database.EnsureDeleted();
        }

        [Benchmark]
        public void EFCoreSelect()
        {
            var contacts = new List<Contact>();

            foreach (var contact in _contactsToMatch)
            {
                contacts.AddRange(_context.Contacts.Where(x => x.CustomerId == contact.CustomerId && x.CountryIsoCode == contact.CountryIsoCode).AsNoTracking().ToList());
            }

            // Console.WriteLine(contacts.Count);
        }

        [Benchmark]
        public void BulkMatch()
        {
            var contacts = _context.BulkMatch(_contactsToMatch,
                x => new { x.CustomerId, x.CountryIsoCode },
                opt =>
                {
                    opt.Timeout = 0;
                });

            // Console.WriteLine(contacts.Count);
        }
    }
}
