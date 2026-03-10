using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class DirectInsertTests : BaseTest
{
    public DirectInsertTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.DirectInsert")
    {
    }

    [Fact]
    public void DirectInsert_Using_Linq_Without_Transaction()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonComplexShippingAddress = new JsonComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonOwnedShippingAddress = new JsonOwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            }
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring
        };

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.DirectInsert(row,
            row => new
            {
                row.Column1,
                row.Column2,
                row.Column3,
                row.Season,
                row.SeasonAsString,
                row.ComplexShippingAddress.Street,
                row.ComplexShippingAddress.Location.Lat,
                row.ComplexShippingAddress.Location.Lng,
                a = row.OwnedShippingAddress.Street,
                b = row.OwnedShippingAddress.Location.Lat,
                c = row.OwnedShippingAddress.Location.Lng,
                row.JsonComplexShippingAddress,
                row.JsonOwnedShippingAddress
            },
            options: options);

        connectionContext.DirectInsert(compositeKeyRow,
            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
            options: options);


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3, dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);
        Assert.Equal(row.ComplexShippingAddress?.Street, dbRows[0].ComplexShippingAddress?.Street);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lat, dbRows[0].ComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lng, dbRows[0].ComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.OwnedShippingAddress?.Street, dbRows[0].OwnedShippingAddress?.Street);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lat, dbRows[0].OwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lng, dbRows[0].OwnedShippingAddress?.Location?.Lng);
        Assert.Equal(row.JsonComplexShippingAddress?.Street, dbRows[0].JsonComplexShippingAddress?.Street);
        Assert.Equal(row.JsonComplexShippingAddress?.Location?.Lat, dbRows[0].JsonComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.JsonComplexShippingAddress?.Location?.Lng, dbRows[0].JsonComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.JsonOwnedShippingAddress?.Street, dbRows[0].JsonOwnedShippingAddress?.Street);
        Assert.Equal(row.JsonOwnedShippingAddress?.Location?.Lat, dbRows[0].JsonOwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.JsonOwnedShippingAddress?.Location?.Lng, dbRows[0].JsonOwnedShippingAddress?.Location?.Lng);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3, dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public void DirectInsert_Using_Linq_With_Transaction_Committed()
    {
        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Summer,
            SeasonAsString = Season.Summer,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonComplexShippingAddress = new JsonComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonOwnedShippingAddress = new JsonOwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            }
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Summer,
            SeasonAsString = Season.Summer
        };

        connectionContext.DirectInsert(row,
             row => new
             {
                 row.Column1,
                 row.Column2,
                 row.Column3,
                 row.Season,
                 row.SeasonAsString,
                 row.ComplexShippingAddress.Street,
                 row.ComplexShippingAddress.Location.Lat,
                 row.ComplexShippingAddress.Location.Lng,
                 a = row.OwnedShippingAddress.Street,
                 b = row.OwnedShippingAddress.Location.Lat,
                 c = row.OwnedShippingAddress.Location.Lng,
                 row.JsonComplexShippingAddress,
                 row.JsonOwnedShippingAddress
             },
             options: new BulkInsertOptions()
             {
                 LogTo = LogTo
             });

        connectionContext.DirectInsert(compositeKeyRow,
 row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
    options: new BulkInsertOptions()
    {
        LogTo = LogTo
    });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3, dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);
        Assert.Equal(row.ComplexShippingAddress?.Street, dbRows[0].ComplexShippingAddress?.Street);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lat, dbRows[0].ComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lng, dbRows[0].ComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.OwnedShippingAddress?.Street, dbRows[0].OwnedShippingAddress?.Street);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lat, dbRows[0].OwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lng, dbRows[0].OwnedShippingAddress?.Location?.Lng);
        Assert.Equal(row.JsonComplexShippingAddress?.Street, dbRows[0].JsonComplexShippingAddress?.Street);
        Assert.Equal(row.JsonComplexShippingAddress?.Location?.Lat, dbRows[0].JsonComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.JsonComplexShippingAddress?.Location?.Lng, dbRows[0].JsonComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.JsonOwnedShippingAddress?.Street, dbRows[0].JsonOwnedShippingAddress?.Street);
        Assert.Equal(row.JsonOwnedShippingAddress?.Location?.Lat, dbRows[0].JsonOwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.JsonOwnedShippingAddress?.Location?.Lng, dbRows[0].JsonOwnedShippingAddress?.Location?.Lng);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3, dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }

    [Fact]
    public void DirectInsert_Using_Linq_With_Transaction_RolledBack()
    {
        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonComplexShippingAddress = new JsonComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonOwnedShippingAddress = new JsonOwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            }
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn
        };

        connectionContext.DirectInsert(row,
             row => new
             {
                 row.Column1,
                 row.Column2,
                 row.Column3,
                 row.Season,
                 row.SeasonAsString,
                 row.ComplexShippingAddress.Street,
                 row.ComplexShippingAddress.Location.Lat,
                 row.ComplexShippingAddress.Location.Lng,
                 a = row.OwnedShippingAddress.Street,
                 b = row.OwnedShippingAddress.Location.Lat,
                 c = row.OwnedShippingAddress.Location.Lng,
                 row.JsonComplexShippingAddress,
                 row.JsonOwnedShippingAddress
             },
      options: new BulkInsertOptions()
      {
          LogTo = LogTo
      });

        connectionContext.DirectInsert(compositeKeyRow,
               row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
       options: new BulkInsertOptions()
       {
           LogTo = LogTo
       });

        tran.Rollback();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Empty(dbRows);
        Assert.Empty(dbCompositeKeyRows);
    }

    [Fact]
    public void DirectInsert_KeepIdentity()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntry = new ConfigurationEntry
        {
            Id = Guid.NewGuid(),
            Key = $"Key1",
            Value = $"Value1",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        connectionContext.DirectInsert(configurationEntry,
         options: new BulkInsertOptions()
         {
             KeepIdentity = true,
             LogTo = LogTo
         });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
        Assert.Single(configurationEntriesInDb);
        Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
        Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
        Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
        Assert.Equal(configurationEntry.CreatedDateTime, configurationEntriesInDb[0].CreatedDateTime);
    }

    [Fact]
    public void DirectInsert_Return_DbGeneratedId()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntry = new ConfigurationEntry
        {
            Key = $"Key1",
            Value = $"Value1",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        connectionContext.DirectInsert(configurationEntry,
                  options: new BulkInsertOptions()
                  {
                      LogTo = LogTo
                  });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
        Assert.Single(configurationEntriesInDb);
        Assert.NotEqual(Guid.Empty, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
        Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
        Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
        Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
        Assert.Equal(configurationEntry.CreatedDateTime, configurationEntriesInDb[0].CreatedDateTime);
    }

    [Fact]
    public void DirectInsert_Using_DynamicString()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var row = new SingleKeyRow<int>
        {
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonComplexShippingAddress = new JsonComplexTypeAddress
            {
                Street = "Street 1",
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            },
            JsonOwnedShippingAddress = new JsonOwnedTypeAddress
            {
                Street = "Street 1",
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128,
                    Lng = -74.0060
                }
            }
        };

        var compositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = 1,
            Id2 = 1,
            Column1 = 1,
            Column2 = "" + 1,
            Column3 = DateTime.Now,
            Season = Season.Spring,
            SeasonAsString = Season.Spring
        };

        var options = new BulkInsertOptions
        {
            LogTo = LogTo
        };

        connectionContext.DirectInsert(row,
            [
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString",
            "ComplexShippingAddress.Street",
            "ComplexShippingAddress.Location.Lat",
            "ComplexShippingAddress.Location.Lng",
            "OwnedShippingAddress.Street",
            "OwnedShippingAddress.Location.Lat",
            "OwnedShippingAddress.Location.Lng",
            "JsonComplexShippingAddress",
            "JsonOwnedShippingAddress"
            ],
            options: options);

        connectionContext.DirectInsert(compositeKeyRow,
            [
            "Id1",
            "Id2",
            "Column1",
            "Column2",
            "Column3",
            "Season",
            "SeasonAsString"
            ],
            options: options);


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);

        Assert.Equal(row.Id, dbRows[0].Id);
        Assert.Equal(row.Column1, dbRows[0].Column1);
        Assert.Equal(row.Column2, dbRows[0].Column2);
        Assert.Equal(row.Column3, dbRows[0].Column3);
        Assert.Equal(row.Season, dbRows[0].Season);
        Assert.Equal(row.SeasonAsString, dbRows[0].SeasonAsString);
        Assert.Equal(row.ComplexShippingAddress?.Street, dbRows[0].ComplexShippingAddress?.Street);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lat, dbRows[0].ComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.ComplexShippingAddress?.Location?.Lng, dbRows[0].ComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.OwnedShippingAddress?.Street, dbRows[0].OwnedShippingAddress?.Street);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lat, dbRows[0].OwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.OwnedShippingAddress?.Location?.Lng, dbRows[0].OwnedShippingAddress?.Location?.Lng);
        Assert.Equal(row.JsonComplexShippingAddress?.Street, dbRows[0].JsonComplexShippingAddress?.Street);
        Assert.Equal(row.JsonComplexShippingAddress?.Location?.Lat, dbRows[0].JsonComplexShippingAddress?.Location?.Lat);
        Assert.Equal(row.JsonComplexShippingAddress?.Location?.Lng, dbRows[0].JsonComplexShippingAddress?.Location?.Lng);
        Assert.Equal(row.JsonOwnedShippingAddress?.Street, dbRows[0].JsonOwnedShippingAddress?.Street);
        Assert.Equal(row.JsonOwnedShippingAddress?.Location?.Lat, dbRows[0].JsonOwnedShippingAddress?.Location?.Lat);
        Assert.Equal(row.JsonOwnedShippingAddress?.Location?.Lng, dbRows[0].JsonOwnedShippingAddress?.Location?.Lng);

        Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
        Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
        Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
        Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
        Assert.Equal(compositeKeyRow.Column3, dbCompositeKeyRows[0].Column3);
        Assert.Equal(compositeKeyRow.Season, dbCompositeKeyRows[0].Season);
        Assert.Equal(compositeKeyRow.SeasonAsString, dbCompositeKeyRows[0].SeasonAsString);
    }
}