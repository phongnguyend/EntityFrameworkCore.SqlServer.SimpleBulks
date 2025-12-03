using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.IListExtensions;

public class ToDataTableTests
{
    private readonly TestDbContext _dbContext;

    public ToDataTableTests()
    {
        _dbContext = new TestDbContext("", "");
    }

    [Fact]
    public Task ToDataTable_SingleKeyRow()
    {
        var rows = new List<SingleKeyRow<int>>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Spring
            });
        }

        var properties = new[]
        {
            "Id",
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString"
        };

        var valueConverters = _dbContext.GetValueConverters(typeof(SingleKeyRow<int>));

        var dataTable = rows.ToDataTable(properties, valueConverters);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            Assert.Equal(row.Id, dataTable.Rows[i]["Id"]);
            Assert.Equal(row.Column1, dataTable.Rows[i]["Column1"]);
            Assert.Equal(row.Column2, dataTable.Rows[i]["Column2"]);
            Assert.Equal(row.Column3, dataTable.Rows[i]["Column3"]);
            Assert.Equal((int)row.Season!, dataTable.Rows[i]["Season"]);
            Assert.Equal(row.SeasonAsString.ToString(), dataTable.Rows[i]["SeasonAsString"]);
        }

        var script = dataTable.GenerateTableDefinition("SingleKeyRows", null, null);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_CompositeKeyRow()
    {
        var rows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Spring
            });
        }

        var properties = new[]
        {
            "Id1",
            "Id2",
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString"
        };

        var valueConverters = _dbContext.GetValueConverters(typeof(CompositeKeyRow<int, int>));

        var dataTable = rows.ToDataTable(properties, valueConverters);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            Assert.Equal(row.Id1, dataTable.Rows[i]["Id1"]);
            Assert.Equal(row.Id2, dataTable.Rows[i]["Id2"]);
            Assert.Equal(row.Column1, dataTable.Rows[i]["Column1"]);
            Assert.Equal(row.Column2, dataTable.Rows[i]["Column2"]);
            Assert.Equal(row.Column3, dataTable.Rows[i]["Column3"]);
            Assert.Equal((int)row.Season!, dataTable.Rows[i]["Season"]);
            Assert.Equal(row.SeasonAsString.ToString(), dataTable.Rows[i]["SeasonAsString"]);
        }

        var script = dataTable.GenerateTableDefinition("CompositeKeyRows", null, null);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_ColumnMapping()
    {
        var rows = new List<ConfigurationEntry>();

        for (int i = 0; i < 100; i++)
        {
            rows.Add(new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var properties = new[]
        {
            "Id",
            "Key",
            "Value",
            "Description",
            "CreatedDateTime"
        };

        var valueConverters = _dbContext.GetValueConverters(typeof(ConfigurationEntry));
        var columnNames = _dbContext.GetColumnNames(typeof(ConfigurationEntry));
        var columnTypes = _dbContext.GetColumnTypes(typeof(ConfigurationEntry));

        var dataTable = rows.ToDataTable(properties, valueConverters);

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            Assert.Equal(row.Id, dataTable.Rows[i]["Id"]);
            Assert.Equal(row.Key, dataTable.Rows[i]["Key"]);
            Assert.Equal(row.Value, dataTable.Rows[i]["Value"]);
            Assert.Equal(row.Description, dataTable.Rows[i]["Description"]);
            Assert.Equal(row.CreatedDateTime, dataTable.Rows[i]["CreatedDateTime"]);
        }

        var script = dataTable.GenerateTableDefinition("ConfigurationEntries", columnNames, columnTypes);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_ComplexType()
    {
        var orders = new List<ComplexTypeOrder>
        {
            new() {},
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                }
            },
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                    Street = "123 Main St"
                }
            },
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                    Location = new ComplexTypeLocation
                    {

                    }
                }
            },
            new()
            {
                ShippingAddress = new ComplexTypeAddress
                {
                    Location = new ComplexTypeLocation
                    {
                        Lat = 40.7128,
                        Lng = -74.0060
                    }
                }
            }
        };

        var properties = new[]
        {
            "Id",
            "ShippingAddress.Street",
            "ShippingAddress.Location.Lat",
            "ShippingAddress.Location.Lng"
        };

        var dataTable = orders.ToDataTable(properties);

        for (int i = 0; i < orders.Count; i++)
        {
            var row = orders[i];
            Assert.Equal(HandleNull(row.Id), dataTable.Rows[i]["Id"]);
            Assert.Equal(HandleNull(row.ShippingAddress?.Street), dataTable.Rows[i]["ShippingAddress.Street"]);
            Assert.Equal(HandleNull(row.ShippingAddress?.Location?.Lat), dataTable.Rows[i]["ShippingAddress.Location.Lat"]);
            Assert.Equal(HandleNull(row.ShippingAddress?.Location?.Lng), dataTable.Rows[i]["ShippingAddress.Location.Lng"]);
        }

        var script = dataTable.GenerateTableDefinition("ComplexTypeOrders", null, null);

        // Assert
        return Verify(script);
    }

    [Fact]
    public Task ToDataTable_OwnedType()
    {
        var orders = new List<OwnedTypeOrder>
        {
            new() {},
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                }
            },
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                    Street = "123 Main St"
                }
            },
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                    Location = new OwnedTypeLocation
                    {

                    }
                }
            },
            new()
            {
                ShippingAddress = new OwnedTypeAddress
                {
                    Location = new OwnedTypeLocation
                    {
                        Lat = 40.7128,
                        Lng = -74.0060
                    }
                }
            }
        };

        var properties = new[]
        {
            "Id",
            "ShippingAddress.Street",
            "ShippingAddress.Location.Lat",
            "ShippingAddress.Location.Lng"
        };

        var dataTable = orders.ToDataTable(properties);

        for (int i = 0; i < orders.Count; i++)
        {
            var row = orders[i];
            Assert.Equal(HandleNull(row.Id), dataTable.Rows[i]["Id"]);
            Assert.Equal(HandleNull(row.ShippingAddress?.Street), dataTable.Rows[i]["ShippingAddress.Street"]);
            Assert.Equal(HandleNull(row.ShippingAddress?.Location?.Lat), dataTable.Rows[i]["ShippingAddress.Location.Lat"]);
            Assert.Equal(HandleNull(row.ShippingAddress?.Location?.Lng), dataTable.Rows[i]["ShippingAddress.Location.Lng"]);
        }

        var script = dataTable.GenerateTableDefinition("OwnedTypeOrders", null, null);

        // Assert
        return Verify(script);
    }

    private object HandleNull(object? value)
    {
        return value ?? DBNull.Value;
    }
}
