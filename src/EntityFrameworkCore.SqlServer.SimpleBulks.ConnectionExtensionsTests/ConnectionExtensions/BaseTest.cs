using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.Data.SqlClient;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    private readonly SqlServerFixture _fixture;
    protected readonly TestDbContext _context;
    protected readonly SqlConnection _connection;

    protected BaseTest(ITestOutputHelper output, SqlServerFixture fixture, string dbPrefixName)
    {
        _output = output;
        _fixture = fixture;
        var connectionString = _fixture.GetConnectionString(dbPrefixName);
        string schema = GetSchema();
        _context = GetDbContext(connectionString, schema);
        _context.Database.EnsureCreated();
        _connection = new SqlConnection(connectionString);

        TableMapper.Configure<SingleKeyRow<int>>(config =>
        {
            config
            .Schema(schema)
            .TableName("SingleKeyRows")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated);
        });

        TableMapper.Configure<CompositeKeyRow<int, int>>(config =>
        {
            config
            .Schema(schema)
            .TableName("CompositeKeyRows")
            .PrimaryKeys(x => new { x.Id1, x.Id2 });
        });

        TableMapper.Configure<ConfigurationEntry>(config =>
        {
            config
            .Schema(schema)
            .TableName("ConfigurationEntry")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated);
        });

        TableMapper.Configure<Customer>(config =>
        {
            config
            .Schema(schema)
            .TableName("Customers")
            .IgnoreProperty(x => x.Contacts);
        });

        TableMapper.Configure<Contact>(config =>
        {
            config
            .Schema(schema)
            .TableName("Contacts").
            IgnoreProperty(x => x.Customer);
        });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string connectionString, string schema)
    {
        return new TestDbContext(connectionString, schema);
    }

    public void LogTo(string log)
    {
        _output.WriteLine(log);
        Console.WriteLine(log);
    }

    protected string GetSchema()
    {
        return Environment.GetEnvironmentVariable("SCHEMA") ?? "";
    }
}
