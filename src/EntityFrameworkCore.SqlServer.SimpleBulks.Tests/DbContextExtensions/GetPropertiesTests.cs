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

    [Fact]
    public Task GetProperties_JsonComplexType_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties = dbContext.GetProperties(typeof(JsonComplexTypeOrder));

        // Assert
        Assert.Equal(2, properties.Count);

        var property = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(int), property.PropertyType);
        Assert.Equal("Id", property.ColumnName);
        Assert.Equal("int", property.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.True(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress");
        Assert.Equal(typeof(ComplexTypeAddress), property.PropertyType);
        Assert.Equal("ShippingAddress", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);
        Assert.True(property.IsJson);
        Assert.NotNull(property.JsonProperties);

        // Verify that the JsonProperties have correct structure
        var jsonProperties = property.JsonProperties;
        var flattenedJsonProperties = property.FlattenedJsonProperties;
        Assert.Equal(2, jsonProperties.Count); // Street and Location

        var streetWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Street");
        Assert.NotNull(streetWriter);
        Assert.Equal("Street", streetWriter.JsonPropertyName);
        Assert.Equal("Street", streetWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Street", streetWriter.FullClrPropertyName);
        Assert.Equal(typeof(string), streetWriter.PropertyType);
        Assert.False(streetWriter.IsNestedComplexType);

        var locationWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Location");
        Assert.NotNull(locationWriter);
        Assert.Equal("xxx", locationWriter.JsonPropertyName); // Mapped to "xxx" in TestDbContext
        Assert.Equal("xxx", locationWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Location", locationWriter.FullClrPropertyName);
        Assert.Equal(typeof(ComplexTypeLocation), locationWriter.PropertyType);
        Assert.True(locationWriter.IsNestedComplexType);
        Assert.NotNull(locationWriter.NestedProperties);
        Assert.Equal(2, locationWriter.NestedProperties.Count); // Lat and Lng

        var latWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lat");
        Assert.NotNull(latWriter);
        Assert.Equal("xxx.Lat", latWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Location.Lat", latWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), latWriter.PropertyType);

        var lngWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lng");
        Assert.NotNull(lngWriter);
        Assert.Equal("xxx.Lng", lngWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Location.Lng", lngWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), lngWriter.PropertyType);

        // Verify serialization using JsonProperties

        var testOrder = new JsonComplexTypeOrder
        {
            Id = 1,
            ShippingAddress = new ComplexTypeAddress
            {
                Street = "123 Main St",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            }
        };

        var json = JsonPropertyWriter.Write(testOrder, flattenedJsonProperties, indented: true);
        Assert.NotNull(json);
        Assert.Contains("Street", json);
        Assert.Contains("123 Main St", json);
        Assert.Contains("xxx", json); // Location is mapped to "xxx"
        Assert.Contains("Lat", json);
        Assert.Contains("Lng", json);

        return Verify(json);
    }



    [Fact]
    public Task GetProperties_JsonOwnedType_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties = dbContext.GetProperties(typeof(JsonOwnedTypeOrder));

        // Assert
        Assert.Equal(2, properties.Count);

        var property = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(int), property.PropertyType);
        Assert.Equal("Id", property.ColumnName);
        Assert.Equal("int", property.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.True(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ShippingAddress");
        Assert.Equal(typeof(OwnedTypeAddress), property.PropertyType);
        Assert.Equal("ShippingAddress", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);
        Assert.True(property.IsJson);
        Assert.NotNull(property.JsonProperties);

        // Verify that the JsonProperties have correct structure
        var jsonProperties = property.JsonProperties;
        var flattenedJsonProperties = property.FlattenedJsonProperties;
        Assert.Equal(3, jsonProperties.Count); // Street and Location

        var streetWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Street");
        Assert.NotNull(streetWriter);
        Assert.Equal("Street", streetWriter.JsonPropertyName);
        Assert.Equal("Street", streetWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Street", streetWriter.FullClrPropertyName);
        Assert.Equal(typeof(string), streetWriter.PropertyType);
        Assert.False(streetWriter.IsNestedComplexType);

        var locationWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Location");
        Assert.NotNull(locationWriter);
        Assert.Equal("xxx", locationWriter.JsonPropertyName); // Mapped to "xxx" in TestDbContext
        Assert.Equal("xxx", locationWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Location", locationWriter.FullClrPropertyName);
        Assert.Equal(typeof(OwnedTypeLocation), locationWriter.PropertyType);
        Assert.True(locationWriter.IsNestedComplexType);
        Assert.NotNull(locationWriter.NestedProperties);
        Assert.Equal(3, locationWriter.NestedProperties.Count); // Lat and Lng

        var latWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lat");
        Assert.NotNull(latWriter);
        Assert.Equal("xxx.Lat", latWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Location.Lat", latWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), latWriter.PropertyType);

        var lngWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lng");
        Assert.NotNull(lngWriter);
        Assert.Equal("xxx.Lng", lngWriter.FullJsonPropertyName);
        Assert.Equal("ShippingAddress.Location.Lng", lngWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), lngWriter.PropertyType);

        // Verify serialization using JsonProperties
        var testOrder = new JsonOwnedTypeOrder
        {
            Id = 1,
            ShippingAddress = new OwnedTypeAddress
            {
                Street = "123 Main St",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            }
        };

        var json = JsonPropertyWriter.Write(testOrder, flattenedJsonProperties, indented: true);
        Assert.NotNull(json);
        Assert.Contains("Street", json);
        Assert.Contains("123 Main St", json);
        Assert.Contains("xxx", json); // Location is mapped to "xxx"
        Assert.Contains("Lat", json);
        Assert.Contains("Lng", json);

        return Verify(json);
    }

    [Fact]
    public Task GetProperties_JsonComplexOwnedType_ReturnsCorrectColumnInformation()
    {
        // Arrange
        var dbContext = new TestDbContext("", "");

        // Act
        var properties = dbContext.GetProperties(typeof(JsonComplexOwnedTypeOrder));

        // Assert
        Assert.Equal(3, properties.Count);

        var property = properties.First(p => p.PropertyName == "Id");
        Assert.Equal(typeof(int), property.PropertyType);
        Assert.Equal("Id", property.ColumnName);
        Assert.Equal("int", property.ColumnType);
        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.True(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);

        property = properties.First(p => p.PropertyName == "ComplexShippingAddress");
        Assert.Equal(typeof(ComplexTypeAddress), property.PropertyType);
        Assert.Equal("ComplexShippingAddress", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);
        Assert.True(property.IsJson);
        Assert.NotNull(property.JsonProperties);

        // Verify that the JsonProperties have correct structure
        var jsonProperties = property.JsonProperties;
        var flattenedJsonProperties = property.FlattenedJsonProperties;
        Assert.Equal(2, jsonProperties.Count); // Street and Location

        var streetWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Street");
        Assert.NotNull(streetWriter);
        Assert.Equal("Street", streetWriter.JsonPropertyName);
        Assert.Equal("Street", streetWriter.FullJsonPropertyName);
        Assert.Equal("ComplexShippingAddress.Street", streetWriter.FullClrPropertyName);
        Assert.Equal(typeof(string), streetWriter.PropertyType);
        Assert.False(streetWriter.IsNestedComplexType);

        var locationWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Location");
        Assert.NotNull(locationWriter);
        Assert.Equal("xxx", locationWriter.JsonPropertyName); // Mapped to "xxx" in TestDbContext
        Assert.Equal("xxx", locationWriter.FullJsonPropertyName);
        Assert.Equal("ComplexShippingAddress.Location", locationWriter.FullClrPropertyName);
        Assert.Equal(typeof(ComplexTypeLocation), locationWriter.PropertyType);
        Assert.True(locationWriter.IsNestedComplexType);
        Assert.NotNull(locationWriter.NestedProperties);
        Assert.Equal(2, locationWriter.NestedProperties.Count); // Lat and Lng

        var latWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lat");
        Assert.NotNull(latWriter);
        Assert.Equal("xxx.Lat", latWriter.FullJsonPropertyName);
        Assert.Equal("ComplexShippingAddress.Location.Lat", latWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), latWriter.PropertyType);

        var lngWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lng");
        Assert.NotNull(lngWriter);
        Assert.Equal("xxx.Lng", lngWriter.FullJsonPropertyName);
        Assert.Equal("ComplexShippingAddress.Location.Lng", lngWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), lngWriter.PropertyType);


        property = properties.First(p => p.PropertyName == "OwnedShippingAddress");
        Assert.Equal(typeof(OwnedTypeAddress), property.PropertyType);
        Assert.Equal("OwnedShippingAddress", property.ColumnName);
        Assert.Equal("nvarchar(max)", property.ColumnType);
        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.Null(property.DefaultValueSql);
        Assert.False(property.IsPrimaryKey);
        Assert.False(property.IsRowVersion);
        Assert.True(property.IsJson);
        Assert.NotNull(property.JsonProperties);

        // Verify that the JsonProperties have correct structure
        jsonProperties = property.JsonProperties;
        flattenedJsonProperties = property.FlattenedJsonProperties;
        Assert.Equal(3, jsonProperties.Count); // Street and Location

        streetWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Street");
        Assert.NotNull(streetWriter);
        Assert.Equal("Street", streetWriter.JsonPropertyName);
        Assert.Equal("Street", streetWriter.FullJsonPropertyName);
        Assert.Equal("OwnedShippingAddress.Street", streetWriter.FullClrPropertyName);
        Assert.Equal(typeof(string), streetWriter.PropertyType);
        Assert.False(streetWriter.IsNestedComplexType);

        locationWriter = jsonProperties.FirstOrDefault(w => w.ClrPropertyName == "Location");
        Assert.NotNull(locationWriter);
        Assert.Equal("xxx", locationWriter.JsonPropertyName); // Mapped to "xxx" in TestDbContext
        Assert.Equal("xxx", locationWriter.FullJsonPropertyName);
        Assert.Equal("OwnedShippingAddress.Location", locationWriter.FullClrPropertyName);
        Assert.Equal(typeof(OwnedTypeLocation), locationWriter.PropertyType);
        Assert.True(locationWriter.IsNestedComplexType);
        Assert.NotNull(locationWriter.NestedProperties);
        Assert.Equal(3, locationWriter.NestedProperties.Count); // Lat and Lng

        latWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lat");
        Assert.NotNull(latWriter);
        Assert.Equal("xxx.Lat", latWriter.FullJsonPropertyName);
        Assert.Equal("OwnedShippingAddress.Location.Lat", latWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), latWriter.PropertyType);

        lngWriter = locationWriter.NestedProperties.FirstOrDefault(w => w.ClrPropertyName == "Lng");
        Assert.NotNull(lngWriter);
        Assert.Equal("xxx.Lng", lngWriter.FullJsonPropertyName);
        Assert.Equal("OwnedShippingAddress.Location.Lng", lngWriter.FullClrPropertyName);
        Assert.Equal(typeof(double), lngWriter.PropertyType);


        // Verify serialization using JsonProperties
        var testOrder = new JsonComplexOwnedTypeOrder
        {
            Id = 1,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "123 Main St",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "123 Main St",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            }
        };

        var json1 = JsonPropertyWriter.Write(testOrder, properties.First(p => p.PropertyName == "ComplexShippingAddress").FlattenedJsonProperties, indented: true);
        Assert.NotNull(json1);
        Assert.Contains("Street", json1);
        Assert.Contains("123 Main St", json1);
        Assert.Contains("xxx", json1); // Location is mapped to "xxx"
        Assert.Contains("Lat", json1);
        Assert.Contains("Lng", json1);

        var json2 = JsonPropertyWriter.Write(testOrder, properties.First(p => p.PropertyName == "OwnedShippingAddress").FlattenedJsonProperties, indented: true);
        Assert.NotNull(json2);
        Assert.Contains("Street", json2);
        Assert.Contains("123 Main St", json2);
        Assert.Contains("xxx", json2); // Location is mapped to "xxx"
        Assert.Contains("Lat", json2);
        Assert.Contains("Lng", json2);

        return Verify(json1 + Environment.NewLine + json2);
    }
}
