using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

public abstract class BaseTest : IDisposable
{
    protected readonly ITestOutputHelper _output;
    private readonly SqlServerFixture _fixture;
    protected readonly TestDbContext _context;
    protected readonly SqlConnection _connection;
    protected readonly SqlTableInfor<SingleKeyRow<int>> _singleKeyRowTableInfor;
    protected readonly SqlTableInfor<CompositeKeyRow<int, int>> _compositeKeyRowTableInfor;
    private string _schema = Environment.GetEnvironmentVariable("SCHEMA") ?? "";
    private bool _enableDiscriminator = (Environment.GetEnvironmentVariable("DISCRIMINATOR") ?? "") == "true";

    protected BaseTest(ITestOutputHelper output, SqlServerFixture fixture, string dbPrefixName)
    {
        _output = output;
        _fixture = fixture;
        var connectionString = _fixture.GetConnectionString(dbPrefixName);
        _context = GetDbContext(connectionString);
        _context.Database.EnsureCreated();
        _connection = new SqlConnection(connectionString);

        _singleKeyRowTableInfor = new SqlTableInfor<SingleKeyRow<int>>(_schema, "SingleKeyRows")
        {
            PrimaryKeys = ["Id"],
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            },
            ColumnTypeMappings = new Dictionary<string, string>
            {
                {"SeasonAsString", "nvarchar(max)" },
                {"Discriminator", _enableDiscriminator ? _context.GetDiscriminator(typeof(SingleKeyRow<int>)).ColumnType : null }
            },
            ValueConverters = new Dictionary<string, ValueConverter>
            {
                {"SeasonAsString", new ValueConverter(typeof(string),x => x.ToString(),v => (Season)Enum.Parse(typeof(Season), (string)v))}
            },
            Discriminator = _enableDiscriminator ? new Discriminator
            {
                PropertyName = "Discriminator",
                PropertyType = typeof(string),
                PropertyValue = "SingleKeyRow<int>",
                ColumnName = "Discriminator",
                ColumnType = _context.GetDiscriminator(typeof(SingleKeyRow<int>)).ColumnType
            } : null
        };

        _compositeKeyRowTableInfor = new SqlTableInfor<CompositeKeyRow<int, int>>(_schema, "CompositeKeyRows")
        {
            PrimaryKeys = ["Id1", "Id2"],
            ColumnTypeMappings = new Dictionary<string, string>
            {
                {"SeasonAsString", "nvarchar(max)" },
                {"Discriminator", _enableDiscriminator ? _context.GetDiscriminator(typeof(CompositeKeyRow<int, int>)).ColumnType : null }
            },
            ValueConverters = new Dictionary<string, ValueConverter>
            {
                {"SeasonAsString", new ValueConverter(typeof(string),x => x.ToString(),v => (Season)Enum.Parse(typeof(Season), (string)v))}
            },
            Discriminator = _enableDiscriminator ? new Discriminator
            {
                PropertyName = "Discriminator",
                PropertyType = typeof(string),
                PropertyValue = "CompositeKeyRow<int, int>",
                ColumnName = "Discriminator",
                ColumnType = _context.GetDiscriminator(typeof(CompositeKeyRow<int, int>)).ColumnType
            } : null
        };

        TableMapper.Configure<SingleKeyRow<int>>(config =>
        {
            config
            .Schema(_schema)
            .TableName("SingleKeyRows")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated)
            .ConfigureProperty(x => x.SeasonAsString, columnType: "nvarchar(max)")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));

            if (_enableDiscriminator)
            {
                config.ConfigureDiscriminator("Discriminator", value: "SingleKeyRow<int>", columnName: "Discriminator", columnType: _context.GetDiscriminator(typeof(SingleKeyRow<int>)).ColumnType);
            }
        });

        TableMapper.Configure<CompositeKeyRow<int, int>>(config =>
        {
            config
            .Schema(_schema)
            .TableName("CompositeKeyRows")
            .PrimaryKeys(x => new { x.Id1, x.Id2 })
            .ConfigureProperty(x => x.SeasonAsString, columnType: "nvarchar(max)")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));

            if (_enableDiscriminator)
            {
                config.ConfigureDiscriminator("Discriminator", value: "CompositeKeyRow<int, int>", columnName: "Discriminator", columnType: _context.GetDiscriminator(typeof(CompositeKeyRow<int, int>)).ColumnType);
            }
        });

        TableMapper.Configure<ConfigurationEntry>(config =>
        {
            config
            .Schema(_schema)
            .TableName("ConfigurationEntry")
            .PrimaryKeys(x => x.Id)
            .OutputId(x => x.Id, OutputIdMode.ServerGenerated)
            .ConfigureProperty(x => x.RowVersion, readOnly: true);

            if (_enableDiscriminator)
            {
                config.ConfigureDiscriminator("Discriminator", value: "ConfigurationEntry", columnName: "Discriminator", columnType: _context.GetDiscriminator(typeof(ConfigurationEntry)).ColumnType);
            }
        });

        TableMapper.Configure<Customer>(config =>
        {
            config
            .Schema(_schema)
            .TableName("Customers")
            .IgnoreProperty(x => x.Contacts)
            .ConfigureProperty(x => x.SeasonAsString, columnType: "nvarchar(max)")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));
        });

        TableMapper.Configure<Contact>(config =>
        {
            config
            .Schema(_schema)
            .TableName("Contacts")
            .IgnoreProperty(x => x.Customer)
            .ConfigureProperty(x => x.SeasonAsString, columnType: "nvarchar(max)")
            .ConfigurePropertyConversion(x => x.SeasonAsString, y => y.ToString(), z => (Season)Enum.Parse(typeof(Season), z));
        });
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }

    protected TestDbContext GetDbContext(string connectionString)
    {
        Console.WriteLine($"Schema: {_schema}, Enable Discriminator: {_enableDiscriminator}");

        return new TestDbContext(connectionString, _schema, _enableDiscriminator);
    }

    public void LogTo(string log)
    {
        _output.WriteLine(log);
        Console.WriteLine(log);
    }
}
