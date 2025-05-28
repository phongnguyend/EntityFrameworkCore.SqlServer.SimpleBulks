using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions;

public class GetPropertiesTests
{
    [Fact]
    public void GetProperties_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties = dbContext.GetProperties(typeof(ConfigurationEntry));

        // Assert
        Assert.Equal(8, properties.Count);

        var idProperty = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(Guid), idProperty.PropertyType);
        Assert.Equal("Id1", idProperty.ColumnName);
        Assert.Equal("uniqueidentifier", idProperty.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
        Assert.Equal("newsequentialid()", idProperty.DefaultValueSql);
        Assert.True(idProperty.IsPrimaryKey);
        Assert.False(idProperty.IsRowVersion);


        var versionProperty = properties.First(p => p.PropertyName == "RowVersion");
        Assert.Equal(typeof(byte[]), versionProperty.PropertyType);
        Assert.Equal("RowVersion", versionProperty.ColumnName);
        Assert.Equal("rowversion", versionProperty.ColumnType);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, versionProperty.ValueGenerated);
        Assert.Null(versionProperty.DefaultValueSql);
        Assert.False(versionProperty.IsPrimaryKey);
        Assert.True(versionProperty.IsRowVersion);
    }
}
