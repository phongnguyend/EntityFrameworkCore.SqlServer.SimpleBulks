using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.TableInforTests;

public class CreateSetStatementWithParameterStyleTests
{
    [Fact]
    public void CreateSetStatement_WithParameterStyle_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetStatement("Value", null);

        // Assert
        Assert.Equal("[Value] = @Value", result);
    }

    [Fact]
    public void CreateSetStatement_WithParameterStyle_AndColumnMapping_ReturnsCorrectStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act - Key is mapped to Key1 column
        var result = tableInfor.CreateSetStatement("Key", null);

        // Assert
        Assert.Equal("[Key1] = @Key", result);
    }

    [Fact]
    public void CreateSetStatement_WithParameterStyle_AndCustomConfigureSetStatement_ReturnsCustomStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetStatement("Value", ctx =>
        {
            return $"{ctx.Left} = COALESCE({ctx.Right}, 'default')";
        });

        // Assert
        Assert.Equal("[Value] = COALESCE(@Value, 'default')", result);
    }

    [Fact]
    public void CreateSetStatement_WithParameterStyle_AndCustomConfigureSetStatementReturnsNull_ReturnsDefaultStatement()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();

        // Act
        var result = tableInfor.CreateSetStatement("Value", ctx => null);

        // Assert
        Assert.Equal("[Value] = @Value", result);
    }

    [Fact]
    public void CreateSetStatement_WithParameterStyle_ConfigureSetStatementReceivesCorrectContext()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<ConfigurationEntry>();
        SetStatementContext? capturedContext = null;

        // Act
        tableInfor.CreateSetStatement("Description", ctx =>
        {
            capturedContext = ctx;
            return null;
        });

        // Assert
        Assert.NotNull(capturedContext);
        Assert.Equal(tableInfor, capturedContext.Value.TableInfor);
        Assert.Equal("Description", capturedContext.Value.PropertyName);
        Assert.Equal("[Description]", capturedContext.Value.Left);
        Assert.Equal("@Description", capturedContext.Value.Right);
        Assert.Null(capturedContext.Value.TargetTableAlias);
        Assert.Null(capturedContext.Value.SourceTableAlias);
    }

    [Fact]
    public void CreateSetStatement_WithParameterStyle_PropertyWithDot_ReturnsCorrectParameterName()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");
        var tableInfor = dbContext.GetTableInfor<OwnedTypeOrder>();

        // Act
        var result = tableInfor.CreateSetStatement("ShippingAddress.Street", null);

        // Assert
        Assert.Equal("[ShippingAddress_Street] = @ShippingAddress_Street", result);
    }
}
