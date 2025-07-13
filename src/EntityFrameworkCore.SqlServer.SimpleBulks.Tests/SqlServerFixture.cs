using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests;

public class SqlServerFixture : IAsyncLifetime
{
    private bool UseContainer => true;

    public MsSqlContainer? Container { get; }

    public SqlServerFixture()
    {
        if (!UseContainer)
        {
            return;
        }

        Container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
    }

    public async Task InitializeAsync()
    {
        if (!UseContainer)
        {
            return;
        }

        await Container!.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (!UseContainer)
        {
            return;
        }

        await Container!.DisposeAsync();
    }

    public string GetConnectionString(string dbPrefixName)
    {
        if (!UseContainer)
        {
            return $"Server=127.0.0.1;Database={dbPrefixName}.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False";
        }

        var connectionString = new SqlConnectionStringBuilder(Container!.GetConnectionString())
        {
            InitialCatalog = $"{dbPrefixName}.{Guid.NewGuid()}"
        }.ToString();

        return connectionString;
    }
}

