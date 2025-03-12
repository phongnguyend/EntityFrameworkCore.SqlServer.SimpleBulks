using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;

    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, string dbPrefixName)
    {
        _output = output;
        _context = GetDbContext(dbPrefixName);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected string GetConnectionString(string dbPrefixName)
    {
        return $"Server=127.0.0.1;Database={dbPrefixName}.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False";
    }

    protected TestDbContext GetDbContext(string dbPrefixName)
    {
        return new TestDbContext(GetConnectionString(dbPrefixName));
    }
}
