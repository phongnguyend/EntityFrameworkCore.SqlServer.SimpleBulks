using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.TableInforTests;

public class CreateSetStatementWithTableAliasesTests
{
    [Fact]
    public void CreateSetStatement_WithTableAliases_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetStatement("Value", "t", "s", null);

        // Assert
        Assert.Equal("t.[Value] = s.[Value]", result);
    }

    [Fact]
    public void CreateSetStatement_WithTableAliases_AndColumnMapping_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act - Key is mapped to Key1 column
        var result = tableInfor.CreateSetStatement("Key", "t", "s", null);

        // Assert
        Assert.Equal("t.[Key1] = s.[Key]", result);
    }

    [Fact]
    public void CreateSetStatement_WithTableAliases_AndCustomConfigureSetStatement_ReturnsCustomStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetStatement("Value", "t", "s", ctx =>
        {
            return $"{ctx.Left} = COALESCE({ctx.Right}, {ctx.TargetTableAlias}.[Value])";
        });

        // Assert
        Assert.Equal("t.[Value] = COALESCE(s.[Value], t.[Value])", result);
    }

    [Fact]
    public void CreateSetStatement_WithTableAliases_AndCustomConfigureSetStatementReturnsNull_ReturnsDefaultStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetStatement("Value", "t", "s", ctx => null);

        // Assert
        Assert.Equal("t.[Value] = s.[Value]", result);
    }

    [Fact]
    public void CreateSetStatement_WithTableAliases_ConfigureSetStatementReceivesCorrectContext()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();
        SetStatementContext? capturedContext = null;

        // Act
        tableInfor.CreateSetStatement("Value", "target", "source", ctx =>
        {
            capturedContext = ctx;
            return null;
        });

        // Assert
        Assert.NotNull(capturedContext);
        Assert.Equal(tableInfor, capturedContext.Value.TableInfor);
        Assert.Equal("Value", capturedContext.Value.PropertyName);
        Assert.Equal("target.[Value]", capturedContext.Value.Left);
        Assert.Equal("source.[Value]", capturedContext.Value.Right);
        Assert.Equal("target", capturedContext.Value.TargetTableAlias);
        Assert.Equal("source", capturedContext.Value.SourceTableAlias);
    }

    [Fact]
    public void CreateSetStatement_WithTableAliases_PropertyWithDot_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<OwnedTypeOrder>();

        // Act
        var result = tableInfor.CreateSetStatement("ShippingAddress.Street", "t", "s", null);

        // Assert
        Assert.Equal("t.[ShippingAddress_Street] = s.[ShippingAddress.Street]", result);
    }
}
