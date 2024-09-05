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
    public class BulkMatchSingleColumnBenchmarks
    {
        private TestDbContext _context;
        private List<Customer> _customers;
        private List<Guid> _customerIds;

        [Params(100, 1000, 10_000, 100_000, 250_000, 500_000, 1_000_000)]
        public int RowsCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _context = new TestDbContext($"Server=127.0.0.1;Database=EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#");
            _context.Database.EnsureCreated();
            _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

            var isoCodes = new string[] { "VN", "US", "GB" };
            var random = new Random(2024);

            _customers = new List<Customer>(RowsCount);

            for (int i = 0; i < RowsCount; i++)
            {
                var customer = new Customer
                {
                    FirstName = "FirstName " + i,
                    LastName = "LastName " + i,
                    Index = i,
                    CurrentCountryIsoCode = isoCodes[random.Next(isoCodes.Length)]
                };
                _customers.Add(customer);
            }

            _context.BulkInsert(_customers);

            _customerIds = _customers.Select(x => x.Id).ToList();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _context.Database.EnsureDeleted();
        }

        [Benchmark]
        public void EFCoreSelect()
        {
            var customers = new List<Customer>();

            foreach (var id in _customerIds)
            {
                customers.Add(_context.Customers.Where(x => x.Id == id).AsNoTracking().First());
            }
        }

        [Benchmark]
        public void EFCoreBatchSelect()
        {
            var pageSize = 10_000;
            var pages = _customerIds.Chunk(pageSize);

            var customers = new List<Customer>();

            foreach (var page in pages)
            {
                customers.AddRange(_context.Customers.Where(x => page.Contains(x.Id)).AsNoTracking().ToList());
            }
        }

        [Benchmark]
        public void BulkMatch()
        {
            var matchedCustomers = _customerIds.Select(x => new Customer { Id = x }).ToList();

            var customers = _context.BulkMatch(matchedCustomers,
                x => x.Id,
                opt =>
                {
                    opt.Timeout = 0;
                });
        }
    }
}
