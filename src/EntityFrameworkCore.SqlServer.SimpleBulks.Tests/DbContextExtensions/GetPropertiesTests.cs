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

    [Fact]
    public void GetProperties_ShouldReturnFromCache()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties1 = dbContext.GetProperties(typeof(ConfigurationEntry));
        var properties2 = dbContext.GetProperties(typeof(ConfigurationEntry));

        // Assert
        Assert.Equal(properties1, properties2);
    }

    [Fact]
    public async Task GetProperties_MultiThreads_ShoudReturnFromCache()
    {
        // Arrange && Act
        var tasks = new List<Task<IReadOnlyList<ColumnInfor>>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                using var dbct = new TestDbContext("", "");
                return dbct.GetProperties(typeof(ConfigurationEntry));
            }));
        }

        await Task.WhenAll(tasks.ToArray());

        var dbContext = new TestDbContext("", "");

        var properties = dbContext.GetProperties(typeof(ConfigurationEntry));

        foreach (var task in tasks)
        {
            // Assert
            Assert.Equal(properties, task.Result);
        }
    }

    [Fact]
    public void GetProperties_ComplexType_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties = dbContext.GetProperties(typeof(ComplexTypeOrder));

        // Assert
        Assert.Equal(4, properties.Count);

        var property = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(int), property.PropertyType);
        Assert.Equal("Id", property.ColumnName);
        Assert.Equal("int", property.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.True(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress.Street");
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal("ShippingAddress_Street", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress.Location.Lat");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("ShippingAddress_Location_Lat", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress.Location.Lng");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("ShippingAddress_Location_Lng", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);
    }

    [Fact]
    public void GetProperties_OwnedType_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties = dbContext.GetProperties(typeof(OwnedTypeOrder));

        // Assert
        Assert.Equal(4, properties.Count);

        var property = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(int), property.PropertyType);
        Assert.Equal("Id", property.ColumnName);
        Assert.Equal("int", property.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.True(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress.Street");
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal("ShippingAddress_Street", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress.Location.Lat");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("ShippingAddress_Location_Lat", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress.Location.Lng");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("ShippingAddress_Location_Lng", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);
    }

    [Fact]
    public void GetProperties_ComplexOwnedType_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties = dbContext.GetProperties(typeof(ComplexOwnedTypeOrder));

        // Assert
        Assert.Equal(7, properties.Count);

        var property = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(int), property.PropertyType);
        Assert.Equal("Id", property.ColumnName);
        Assert.Equal("int", property.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.True(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ComplexShippingAddress.Street");
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal("ComplexShippingAddress_Street", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ComplexShippingAddress.Location.Lat");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("ComplexShippingAddress_Location_Lat", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ComplexShippingAddress.Location.Lng");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("ComplexShippingAddress_Location_Lng", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "OwnedShippingAddress.Street");
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal("OwnedShippingAddress_Street", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "OwnedShippingAddress.Location.Lat");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("OwnedShippingAddress_Location_Lat", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "OwnedShippingAddress.Location.Lng");
        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Equal("OwnedShippingAddress_Location_Lng", property.ColumnName);
        Assert.Equal("float", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);
    }
}
