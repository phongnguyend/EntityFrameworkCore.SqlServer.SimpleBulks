using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.ExpressionExtensions;

public class GetMemberNameTests
{
    [Fact]
    public void GetMemberNames_ShouldReturnNull_WhenExpressionIsNewExpression()
    {
        // Arrange
        Expression<Func<Person, object>> expression = x => new { x.FirstName, x.LastName, x.Address, x.Address.Country };

        // Act
        var result = expression.Body.GetMemberName();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMemberNames_ShouldReturnValue_WhenExpressionIsNotNewExpression()
    {
        // Arrange
        Expression<Func<Person, object>> expression1 = x => x.FirstName;
        Expression<Func<Person, object>> expression2 = x => x.Address;
        Expression<Func<Person, object>> expression3 = x => x.Address.Country;

        // Act
        var result1 = expression1.Body.GetMemberName();
        var result2 = expression2.Body.GetMemberName();
        var result3 = expression3.Body.GetMemberName();

        // Assert
        Assert.Equal("FirstName", result1);
        Assert.Equal("Address", result2);
        Assert.Equal("Address.Country", result3);
    }
}
