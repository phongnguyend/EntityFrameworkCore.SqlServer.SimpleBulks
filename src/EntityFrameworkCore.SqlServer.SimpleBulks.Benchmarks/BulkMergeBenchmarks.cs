using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class BulkMergeBenchmarks
{
    private TestDbContext _context;
    private List<Customer> _customers;
    private List<Guid> _customerIds;
    private List<Customer> _newCustomers;

    [Params(100, 1000, 10_000, 100_000, 250_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
        _context.Database.EnsureCreated();
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

        _customers = new List<Customer>(RowsCount);
        _newCustomers = new List<Customer>(RowsCount);

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

        for (int i = RowsCount; i < RowsCount * 2; i++)
        {
            var customer = new Customer
            {
                FirstName = "FirstName " + i,
                LastName = "LastName " + i,
                Index = i,
            };

            _newCustomers.Add(customer);
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void EFCoreUpsert()
    {
        var pageSize = 10_000;
        var pages = _customerIds.Chunk(pageSize);

        var random = new Random(2024);

        foreach (var page in pages)
        {
            var customers = _context.Customers.Where(x => page.Contains(x.Id)).ToList();

            foreach (var customer in customers)
            {
                customer.FirstName = "Updated" + random.Next();
            }
        }

        _context.AddRange(_newCustomers);

        _context.SaveChanges();
    }

    [Benchmark]
    public void BulkMerge()
    {
        var random = new Random(2024);

        foreach (var customer in _customers)
        {
            customer.FirstName = "Updated" + random.Next();
        }

        var customers = new List<Customer>(RowsCount * 2);
        customers.AddRange(_customers);
        customers.AddRange(_newCustomers);

        _context.BulkMerge(customers,
            x => x.Id,
            x => new { x.FirstName },
            x => new { x.FirstName, x.LastName, x.Index },
            opt =>
            {
                opt.Timeout = 0;
            });
    }
}
