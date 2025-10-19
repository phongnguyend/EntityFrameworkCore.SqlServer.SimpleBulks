using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextAsyncExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly SqlServerFixture _fixture;
    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, SqlServerFixture fixture, string dbPrefixName, string schema = "")
    {
        _output = output;
        _fixture = fixture;
        _context = GetDbContext(dbPrefixName, schema);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string dbPrefixName, string schema)
    {
        return new TestDbContext(_fixture.GetConnectionString(dbPrefixName), schema);
    }
}
