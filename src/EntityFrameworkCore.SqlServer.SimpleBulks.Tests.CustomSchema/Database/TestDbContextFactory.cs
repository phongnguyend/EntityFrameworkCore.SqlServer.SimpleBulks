using Microsoft.EntityFrameworkCore.Design;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

internal class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(string[] args)
    {
        return new TestDbContext("Server=.;Database=EFCoreSimpleBulksTests;User Id=sa;Password=sqladmin123!@#;Encrypt=False");
    }
}
