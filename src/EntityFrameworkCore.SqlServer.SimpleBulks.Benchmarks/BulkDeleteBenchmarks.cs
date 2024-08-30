using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks
{
    [WarmupCount(0)]
    [IterationCount(1)]
    [InvocationCount(1)]
    [MemoryDiagnoser]
    public class BulkDeleteBenchmarks
    {
        private TestDbContext _context;
        private List<Customer> _customers;
        private List<Guid> _customerIds;

        [Params(100, 1000, 10_000, 20_000)]
        public int RowsCount { get; set; }

        [IterationSetup]
        public void IterationSetup()
        {
            _context = new TestDbContext($"Server=127.0.0.1;Database=EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#");
            _context.Database.EnsureCreated();
            _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

            _customers = new List<Customer>(RowsCount);

            for (int i = 0; i < RowsCount; i++)
            {
                var customer = new Customer
                {
                    FirstName = "FirstName " + i,
                    LastName = "LastName " + i,
                    Index = i,
                };
                _customers.Add(customer);
            }

            _context.BulkInsert(_customers);

            _customerIds = _customers.Select(x => x.Id).ToList();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _context.Database.EnsureDeleted();
        }

        [Benchmark]
        public void EFCoreDelete()
        {
            var pageSize = 10_000;
            var pages = _customerIds.Chunk(pageSize);

            foreach (var page in pages)
            {
                var customers = _context.Customers.Where(x => page.Contains(x.Id)).ToList();

                _context.RemoveRange(customers);
            }

            _context.SaveChanges();
        }

        [Benchmark]
        public void BulkDelete()
        {
            var customers = _customerIds.Select(x => new Customer { Id = x }).ToList();

            _context.BulkDelete(customers,
                opt =>
                {
                    opt.Timeout = 0;
                });
        }
    }
}
