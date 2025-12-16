using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("SqlServerCollection")]
public class BulkInsertAsyncTests : BaseTest
{
    public BulkInsertAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkInsert")
    {
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Insert_Using_Linq_Without_Transaction(int length)
    {
        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn,
                SeasonAsString = Season.Autumn,
                ComplexShippingAddress = new ComplexTypeAddress
                {
                    Street = "Street " + i,
                    Location = new ComplexTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                },
                OwnedShippingAddress = new OwnedTypeAddress
                {
                    Street = "Street " + i,
                    Location = new OwnedTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                }
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn,
                SeasonAsString = Season.Autumn
            });
        }

        await _context.BulkInsertAsync(rows,
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
                    c = row.OwnedShippingAddress.Location.Lng
                },
                new BulkInsertOptions()
                {
                    LogTo = LogTo
                });

        await _context.BulkInsertAsync(compositeKeyRows,
       row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkInsertOptions()
                {
                    LogTo = LogTo
                });


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);
            Assert.Equal(rows[i].ComplexShippingAddress?.Street, dbRows[i].ComplexShippingAddress?.Street);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lat, dbRows[i].ComplexShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lng, dbRows[i].ComplexShippingAddress?.Location?.Lng);
            Assert.Equal(rows[i].OwnedShippingAddress?.Street, dbRows[i].OwnedShippingAddress?.Street);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lat, dbRows[i].OwnedShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lng, dbRows[i].OwnedShippingAddress?.Location?.Lng);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
            Assert.Equal(compositeKeyRows[i].SeasonAsString, dbCompositeKeyRows[i].SeasonAsString);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Insert_Using_Linq_With_Transaction_Committed(int length)
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Spring,
                SeasonAsString = Season.Spring,
                ComplexShippingAddress = new ComplexTypeAddress
                {
                    Street = "Street " + i,
                    Location = new ComplexTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                },
                OwnedShippingAddress = new OwnedTypeAddress
                {
                    Street = "Street " + i,
                    Location = new OwnedTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                }
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
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

        await _context.BulkInsertAsync(rows,
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
          c = row.OwnedShippingAddress.Location.Lng
      });

        await _context.BulkInsertAsync(compositeKeyRows,
      row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);
            Assert.Equal(rows[i].ComplexShippingAddress?.Street, dbRows[i].ComplexShippingAddress?.Street);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lat, dbRows[i].ComplexShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lng, dbRows[i].ComplexShippingAddress?.Location?.Lng);
            Assert.Equal(rows[i].OwnedShippingAddress?.Street, dbRows[i].OwnedShippingAddress?.Street);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lat, dbRows[i].OwnedShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lng, dbRows[i].OwnedShippingAddress?.Location?.Lng);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
            Assert.Equal(compositeKeyRows[i].SeasonAsString, dbCompositeKeyRows[i].SeasonAsString);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Insert_Using_Linq_With_Transaction_RolledBack(int length)
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Summer,
                SeasonAsString = Season.Summer,
                ComplexShippingAddress = new ComplexTypeAddress
                {
                    Street = "Street " + i,
                    Location = new ComplexTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                },
                OwnedShippingAddress = new OwnedTypeAddress
                {
                    Street = "Street " + i,
                    Location = new OwnedTypeLocation
                    {
                        Lat = 40.7128 + i,
                        Lng = -74.0060 - i
                    }
                }
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Summer,
                SeasonAsString = Season.Summer
            });
        }

        await _context.BulkInsertAsync(rows,
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
                c = row.OwnedShippingAddress.Location.Lng
            });

        await _context.BulkInsertAsync(compositeKeyRows,
            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

        tran.Rollback();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Empty(dbRows);
        Assert.Empty(dbCompositeKeyRows);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Insert_KeepIdentity(int length)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        for (var i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        await _context.BulkInsertAsync(configurationEntries,
       new BulkInsertOptions()
       {
           KeepIdentity = true,
           LogTo = LogTo
       });

        // Assert
        configurationEntries = configurationEntries.OrderBy(x => x.Id).ToList();
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList().OrderBy(x => x.Id).ToList();

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Insert_Return_DbGeneratedId(int length)
    {
        var configurationEntries = new List<ConfigurationEntry>();

        for (var i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        await _context.BulkInsertAsync(configurationEntries,
      new BulkInsertOptions()
      {
          LogTo = LogTo
      });

        // Assert
        configurationEntries = configurationEntries.OrderBy(x => x.Id).ToList();
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList().OrderBy(x => x.Id).ToList();

        for (var i = 0; i < length; i++)
        {
            Assert.NotEqual(Guid.Empty, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
        }
    }
}