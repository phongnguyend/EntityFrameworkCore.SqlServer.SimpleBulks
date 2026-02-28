using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.SetStatementContextTests;

public class SetClauseContextTests
{
    [Fact]
    public void GetTargetTableColumn_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var table = dbContext.GetTableInfor<ConfigurationEntry>();

        var ctx = new SetClauseContext
        {
            TableInfor = table,
        };

        // Assert
        Assert.Equal("[Id1]", ctx.GetTargetTableColumn("Id"));
        Assert.Equal("[Key1]", ctx.GetTargetTableColumn("Key"));
        Assert.Equal("[Value]", ctx.GetTargetTableColumn("Value"));
        Assert.Equal("[Description]", ctx.GetTargetTableColumn("Description"));
    }

    [Fact]
    public void GetTargetTableColumn_WithTargetTableAlias_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var table = dbContext.GetTableInfor<ConfigurationEntry>();

        var ctx = new SetClauseContext
        {
            TableInfor = table,
            TargetTableAlias = "t",
        };

        // Assert
        Assert.Equal("t.[Id1]", ctx.GetTargetTableColumn("Id"));
        Assert.Equal("t.[Key1]", ctx.GetTargetTableColumn("Key"));
        Assert.Equal("t.[Value]", ctx.GetTargetTableColumn("Value"));
        Assert.Equal("t.[Description]", ctx.GetTargetTableColumn("Description"));
    }

    [Fact]
    public void GetSourceTableColumn_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var table = dbContext.GetTableInfor<ConfigurationEntry>();

        var ctx = new SetClauseContext
        {
            TableInfor = table,
        };

        // Assert
        Assert.Equal("@__Id", ctx.GetSourceTableColumn("Id"));
        Assert.Equal("@__Key", ctx.GetSourceTableColumn("Key"));
        Assert.Equal("@__Value", ctx.GetSourceTableColumn("Value"));
        Assert.Equal("@__Description", ctx.GetSourceTableColumn("Description"));
    }

    [Fact]
    public void GetSourceTableColumn_WithSourceTableAlias_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var table = dbContext.GetTableInfor<ConfigurationEntry>();

        var ctx = new SetClauseContext
        {
            TableInfor = table,
            SourceTableAlias = "s",
        };

        // Assert
        Assert.Equal("s.[Id]", ctx.GetSourceTableColumn("Id"));
        Assert.Equal("s.[Key]", ctx.GetSourceTableColumn("Key"));
        Assert.Equal("s.[Value]", ctx.GetSourceTableColumn("Value"));
        Assert.Equal("s.[Description]", ctx.GetSourceTableColumn("Description"));
    }
}
