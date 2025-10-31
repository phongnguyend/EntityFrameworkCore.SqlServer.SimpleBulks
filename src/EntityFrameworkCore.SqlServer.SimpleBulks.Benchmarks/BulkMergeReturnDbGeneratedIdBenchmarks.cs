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
public class BulkMergeReturnDbGeneratedIdBenchmarks1
{
    private TestDbContext _context;
    private List<Customer> _customers;
    private List<Customer> _newCustomers;
    private List<Customer> _allCustomers;

    [Params(100, 1000, 10_000, 100_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
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

        _allCustomers = new List<Customer>(RowsCount * 2);
        _allCustomers.AddRange(_customers);
        _allCustomers.AddRange(_newCustomers);

        var random = new Random(2024);

        foreach (var customer in _customers)
        {
            customer.FirstName = "Updated" + random.Next();
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void ReturnDbGeneratedId()
    {
        _context.BulkMerge(_allCustomers,
      x => x.Id,
   x => new { x.FirstName },
            x => new { x.FirstName, x.LastName, x.Index },
       new BulkMergeOptions()
       {
           Timeout = 0,
           ReturnDbGeneratedId = true
       });
    }

    [Benchmark]
    public void NotReturnDbGeneratedId()
    {
        _context.BulkMerge(_allCustomers,
        x => x.Id,
x => new { x.FirstName },
            x => new { x.FirstName, x.LastName, x.Index },
  new BulkMergeOptions()
  {
      Timeout = 0,
      ReturnDbGeneratedId = false
  });
    }
}

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class BulkMergeReturnDbGeneratedIdBenchmarks2
{
    private TestDbContext _context;
    private List<Customer> _customers;
    private List<Customer> _newCustomers;
    private List<Customer> _allCustomers;

    [Params(250_000, 500_000, 1_000_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
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

        _allCustomers = new List<Customer>(RowsCount * 2);
        _allCustomers.AddRange(_customers);
        _allCustomers.AddRange(_newCustomers);

        var random = new Random(2024);

        foreach (var customer in _customers)
        {
            customer.FirstName = "Updated" + random.Next();
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void ReturnDbGeneratedId()
    {
        _context.BulkMerge(_allCustomers,
            x => x.Id,
 x => new { x.FirstName },
x => new { x.FirstName, x.LastName, x.Index },
      new BulkMergeOptions()
      {
          Timeout = 0,
          ReturnDbGeneratedId = true
      });
    }

    [Benchmark]
    public void NotReturnDbGeneratedId()
    {
        _context.BulkMerge(_allCustomers,
 x => x.Id,
            x => new { x.FirstName },
x => new { x.FirstName, x.LastName, x.Index },
          new BulkMergeOptions()
          {
              Timeout = 0,
              ReturnDbGeneratedId = false
          });
    }
}
