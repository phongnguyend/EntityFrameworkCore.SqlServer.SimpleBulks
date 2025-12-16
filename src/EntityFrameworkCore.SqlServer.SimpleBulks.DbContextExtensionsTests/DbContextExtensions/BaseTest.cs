using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    protected readonly SqlServerFixture _fixture;
    protected readonly TestDbContext _context;

    protected BaseTest(ITestOutputHelper output, SqlServerFixture fixture, string dbPrefixName)
    {
        _output = output;
        _fixture = fixture;
        _context = GetDbContext(dbPrefixName);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string dbPrefixName)
    {
        string schema = Environment.GetEnvironmentVariable("SCHEMA") ?? "";
        bool enableDiscriminator = (Environment.GetEnvironmentVariable("DISCRIMINATOR") ?? "") == "true";

        Console.WriteLine($"Schema: {schema}, Enable Discriminator: {enableDiscriminator}");

        return new TestDbContext(_fixture.GetConnectionString(dbPrefixName), schema, enableDiscriminator);
    }

    public void LogTo(string log)
    {
        _output.WriteLine(log);
        Console.WriteLine(log);
    }
}
