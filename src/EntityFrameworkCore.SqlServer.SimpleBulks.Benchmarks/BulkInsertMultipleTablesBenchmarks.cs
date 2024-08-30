using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks
{
    [WarmupCount(0)]
    [IterationCount(1)]
    [InvocationCount(1)]
    [MemoryDiagnoser]
    public class BulkInsertMultipleTablesBenchmarks
    {
        private TestDbContext _context;
        private List<Customer> _customers;
        private List<Contact> _contacts;

        [Params(100, 1000, 10_000, 100_000)]
        public int RowsCount { get; set; }

        [IterationSetup]
        public void IterationSetup()
        {
            _context = new TestDbContext($"Server=127.0.0.1;Database=EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#");
            _context.Database.EnsureCreated();

            _customers = new List<Customer>(RowsCount);

            for (int i = 0; i < RowsCount; i++)
            {
                var customer = new Customer
                {
                    FirstName = "FirstName " + i,
                    LastName = "LastName " + i,
                    Index = i,
                };

                customer.Contacts = new List<Contact>();

                for (int j = 0; j < 5; j++)
                {
                    customer.Contacts.Add(new Contact
                    {
                        EmailAddress = $"EmailAddress {i} - {j}",
                        PhoneNumber = $"PhoneNumber {i} - {j}",
                        IsDefault = j % 2 == 0,
                        Index = j,
                    });
                }

                _customers.Add(customer);
            }

            _contacts = _customers.SelectMany(x => x.Contacts).ToList();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _context.Database.EnsureDeleted();
        }

        [Benchmark]
        public void EFCoreInsert()
        {
            _context.AddRange(_customers);
            _context.SaveChanges();
        }

        [Benchmark]
        public void BulkInsert()
        {
            _context.BulkInsert(_customers, opt =>
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

            _context.BulkInsert(_contacts, opt =>
            {
                opt.Timeout = 0;
            });
        }
    }
}
