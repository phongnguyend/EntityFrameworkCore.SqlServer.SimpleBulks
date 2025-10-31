using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class BulkDeleteBenchmarks1
{
    private TestDbContext _context;
    private List<Customer> _customers;
    private List<Guid> _customerIds;
    private List<Customer> _customersToDelete;

    [Params(100, 1000, 10_000, 20_000, 50_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
        _context.Database.EnsureCreated();
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

        _customers = new List<Customer>(RowsCount * 2);

        for (int i = 0; i < RowsCount * 2; i++)
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

        _customerIds = _customers.Take(RowsCount).Select(x => x.Id).ToList();

        _customersToDelete = _customerIds.Select(x => new Customer { Id = x }).ToList();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void EFCoreDelete()
    {
        _context.RemoveRange(_customersToDelete);

        _context.SaveChanges();
    }

    [Benchmark]
    public void BulkDelete()
    {
        _context.BulkDelete(_customersToDelete,
            new BulkDeleteOptions()
            {
                Timeout = 0
            });
    }
}

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class BulkDeleteBenchmarks2
{
    private TestDbContext _context;
    private List<Customer> _customers;
    private List<Guid> _customerIds;
    private List<Customer> _customersToDelete;

    [Params(100_000, 250_000, 500_000, 1_000_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
        _context.Database.EnsureCreated();
        _context.Database.SetCommandTimeout(TimeSpan.FromMinutes(2));

        _customers = new List<Customer>(RowsCount * 2);

        for (int i = 0; i < RowsCount * 2; i++)
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

        _customerIds = _customers.Take(RowsCount).Select(x => x.Id).ToList();

        _customersToDelete = _customerIds.Select(x => new Customer { Id = x }).ToList();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void BulkDelete()
    {
        _context.BulkDelete(_customersToDelete,
            new BulkDeleteOptions()
            {
                Timeout = 0
            });
    }
}
