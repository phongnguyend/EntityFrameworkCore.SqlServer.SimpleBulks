using BenchmarkDotNet.Attributes;
using EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;

[WarmupCount(0)]
[IterationCount(1)]
[InvocationCount(1)]
[MemoryDiagnoser]
public class TempTableBenchmarks
{
    private TestDbContext _context;
    private List<Customer> _customers;

    [Params(100, 1000, 10_000, 100_000, 250_000, 500_000, 1_000_000)]
    public int RowsCount { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
        _context = new TestDbContext($"Server=127.0.0.1;Database=EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
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
            _customers.Add(customer);
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _context.Database.EnsureDeleted();
    }

    [Benchmark]
    public void CreateTempTable()
    {
        _context.CreateTempTable(_customers, opt =>
        {
            opt.Timeout = 0;
        });
    }
}
