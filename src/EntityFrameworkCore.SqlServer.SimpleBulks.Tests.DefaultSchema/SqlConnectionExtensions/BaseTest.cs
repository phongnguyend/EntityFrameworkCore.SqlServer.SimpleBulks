using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.Data.SqlClient;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.SqlConnectionExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly TestDbContext _context;
    protected readonly SqlConnection _connection;

    protected BaseTest(ITestOutputHelper output, string dbPrefixName)
    {
        _output = output;
        var connectionString = GetConnectionString(dbPrefixName);
        _context = GetDbContext(connectionString);
        _context.Database.EnsureCreated();
        _connection = new SqlConnection(connectionString);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected string GetConnectionString(string dbPrefixName)
    {
        return $"Server=127.0.0.1;Database={dbPrefixName}.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False";
    }

    protected TestDbContext GetDbContext(string connectionString)
    {
        return new TestDbContext(connectionString);
    }
}
