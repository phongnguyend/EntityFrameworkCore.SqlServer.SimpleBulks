using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.ExpressionExtensions;

public class GetMemberNamesTests
{
    [Fact]
    public void GetMemberNames_ShouldReturnEmptyList_WhenExpressionIsNotNewExpression()
    {
        // Arrange
        Expression<Func<Person, object>> expression = x => x.FirstName;

        // Act
        var result = expression.Body.GetMemberNames();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void GetMemberNames_ShouldReturnList_WhenExpressionIsNewExpression()
    {
        // Arrange
        Expression<Func<Person, object>> expression = x => new { x.FirstName, x.LastName, x.Address, x.Address.Country };

        // Act
        var result = expression.Body.GetMemberNames();

        // Assert
        Assert.Equal("FirstName", result[0]);
        Assert.Equal("LastName", result[1]);
        Assert.Equal("Address", result[2]);
        Assert.Equal("Address.Country", result[3]);
    }
}
