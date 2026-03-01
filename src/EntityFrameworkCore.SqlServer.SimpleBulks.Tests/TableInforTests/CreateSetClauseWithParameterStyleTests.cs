using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.TableInforTests;

public class CreateSetClauseWithParameterStyleTests
{
    [Fact]
    public void CreateSetClause_WithParameterStyle_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetClause("Value", null);

        // Assert
        Assert.Equal("[Value] = @__Value", result);
    }

    [Fact]
    public void CreateSetClause_WithParameterStyle_AndColumnMapping_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act - Key is mapped to Key1 column
        var result = tableInfor.CreateSetClause("Key", null);

        // Assert
        Assert.Equal("[Key1] = @__Key", result);
    }

    [Fact]
    public void CreateSetClause_WithParameterStyle_AndCustomConfigureSetStatement_ReturnsCustomStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetClause("Value", ctx =>
        {
            return $"{ctx.Left} = COALESCE({ctx.Right}, 'default')";
        });

        // Assert
        Assert.Equal("[Value] = COALESCE(@__Value, 'default')", result);
    }

    [Fact]
    public void CreateSetClause_WithParameterStyle_AndCustomConfigureSetStatementReturnsNull_ReturnsDefaultStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetClause("Value", ctx => null);

        // Assert
        Assert.Equal("[Value] = @__Value", result);
    }

    [Fact]
    public void CreateSetClause_WithParameterStyle_ConfigureSetStatementReceivesCorrectContext()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();
        SetClauseContext? capturedContext = null;

        // Act
        tableInfor.CreateSetClause("Description", ctx =>
        {
            capturedContext = ctx;
            return null;
        });

        // Assert
        Assert.NotNull(capturedContext);
        Assert.Equal(tableInfor, capturedContext.Value.TableInfor);
        Assert.Equal("Description", capturedContext.Value.PropertyName);
        Assert.Equal("[Description]", capturedContext.Value.Left);
        Assert.Equal("@__Description", capturedContext.Value.Right);
        Assert.Null(capturedContext.Value.TargetTableAlias);
        Assert.Null(capturedContext.Value.SourceTableAlias);
    }

    [Fact]
    public void CreateSetClause_WithParameterStyle_PropertyWithDot_ReturnsCorrectParameterName()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<OwnedTypeOrder>();

        // Act
        var result = tableInfor.CreateSetClause("ShippingAddress.Street", null);

        // Assert
        Assert.Equal("[ShippingAddress_Street] = @__ShippingAddress_Street", result);
    }
}
