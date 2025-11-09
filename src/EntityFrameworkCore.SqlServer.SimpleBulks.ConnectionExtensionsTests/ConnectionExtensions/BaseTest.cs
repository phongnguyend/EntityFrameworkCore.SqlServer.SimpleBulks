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

    protected BaseTest(ITestOutputHelper output, SqlServerFixture fixture, string dbPrefixName, string schema = "")
    {
        _output = output;
        _fixture = fixture;
        var connectionString = _fixture.GetConnectionString(dbPrefixName);
        _context = GetDbContext(connectionString, schema);
        _context.Database.EnsureCreated();
        _connection = new SqlConnection(connectionString);

        TableMapper.Register<SingleKeyRow<int>>(new SqlTableInfor(schema, "SingleKeyRows")
        {
            PrimaryKeys = ["Id"],
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            }
        });

        TableMapper.Register<CompositeKeyRow<int, int>>(new SqlTableInfor(schema, "CompositeKeyRows")
        {
            PrimaryKeys = ["Id1", "Id2"],
        });

        TableMapper.Register<ConfigurationEntry>(new SqlTableInfor(schema, "ConfigurationEntry")
        {
            PrimaryKeys = ["Id"],
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            }
        });

        TableMapper.Register<Customer>(new SqlTableInfor(schema, "Customers")
        {
            PropertyNames = ["Id", "FirstName", "LastName", "CurrentCountryIsoCode", "Index", "Season", "SeasonAsString"]
        });

        TableMapper.Register<Contact>(new SqlTableInfor(schema, "Contacts")
        {
            PropertyNames = ["Id", "EmailAddress", "PhoneNumber", "CountryIsoCode", "Index", "Season", "SeasonAsString", "CustomerId"]
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
}
