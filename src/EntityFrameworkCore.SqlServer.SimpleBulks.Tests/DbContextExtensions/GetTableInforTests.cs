using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions;

public class GetTableInforTests
{
    [Fact]
    public void GetTableInfor_ReturnsCorrectTableInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var tableInfor = dbContext.GetTableInfor(typeof(ConfigurationEntry));

        // Assert
        Assert.Equal("ConfigurationEntry", tableInfor.Name);
        Assert.Equal("[ConfigurationEntry]", tableInfor.SchemaQualifiedTableName);
    }

    [Fact]
    public void GetTableInfor_ReturnsFromCache()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var tableInfor1 = dbContext.GetTableInfor(typeof(ConfigurationEntry));
        var tableInfor2 = dbContext.GetTableInfor(typeof(ConfigurationEntry));

        // Assert
        Assert.Equal(tableInfor1, tableInfor2);
    }

    [Fact]
    public async Task GetTableInfor_MultiThreads_ShoudReturnFromCache()
    {
        // Arrange && Act
        var tasks = new List<Task<TableInfor>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                using var dbct = new TestDbContext("", "");
                return dbct.GetTableInfor(typeof(ConfigurationEntry));
            }));
        }

        await Task.WhenAll(tasks.ToArray());

        var dbContext = new TestDbContext("", "");

        var tableInfor = dbContext.GetTableInfor(typeof(ConfigurationEntry));

        foreach (var task in tasks)
        {
            // Assert
            Assert.Equal(tableInfor, task.Result);
        }
    }
}
