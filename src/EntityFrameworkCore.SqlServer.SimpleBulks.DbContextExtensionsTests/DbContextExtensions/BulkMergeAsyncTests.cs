using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("SqlServerCollection")]
public class BulkMergeAsyncTests : BaseTest
{
    public BulkMergeAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkMerge")
    {
    }

    private async Task SeedData(int length)
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
                Season = Season.Winter,
                SeasonAsString = Season.Winter,
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
                Season = Season.Winter,
                SeasonAsString = Season.Winter
            });
        }

        await _context.BulkInsertAsync(rows);

        await _context.BulkInsertAsync(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(100, 100)]
    public async Task BulkMerge_Using_Linq_With_Transaction(int length, int insertLength)
    {
        await SeedData(length);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Spring;
            row.SeasonAsString = Season.Spring;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Spring;
            row.SeasonAsString = Season.Spring;
        }

        for (int i = length; i < length + insertLength; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "Inserted using Merge" + i,
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
                Column2 = "Inserted using Merge" + i,
                Column3 = DateTime.Now,
                Season = Season.Summer,
                SeasonAsString = Season.Summer
            });
        }

        var result1 = await _context.BulkMergeAsync(rows,
                row => row.Id,
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
                new BulkMergeOptions()
                {
                    LogTo = LogTo,
                    ReturnDbGeneratedId = true
                });

        var result2 = await _context.BulkMergeAsync(compositeKeyRows,
                row => new { row.Id1, row.Id2 },
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                new BulkMergeOptions()
                {
                    LogTo = LogTo
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length + insertLength, result1.AffectedRows);
        Assert.Equal(insertLength, result1.InsertedRows);
        Assert.Equal(length, result1.UpdatedRows);

        Assert.Equal(length + insertLength, result2.AffectedRows);
        Assert.Equal(insertLength, result2.InsertedRows);
        Assert.Equal(length, result2.UpdatedRows);

        for (var i = 0; i < length + insertLength; i++)
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
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(100, 100)]
    public async Task BulkMerge_Using_Dynamic_String_With_Transaction(int length, int insertLength)
    {
        await SeedData(length);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Spring;
            row.SeasonAsString = Season.Spring;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Spring;
            row.SeasonAsString = Season.Spring;
        }

        for (int i = length; i < length + insertLength; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "Inserted using Merge" + i,
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
                Column2 = "Inserted using Merge" + i,
                Column3 = DateTime.Now,
                Season = Season.Summer,
                SeasonAsString = Season.Summer
            });
        }

        var result1 = await _context.BulkMergeAsync(rows,
            ["Id"],
            ["Column1", "Column2", "Column3", "Season", "SeasonAsString",
          "ComplexShippingAddress.Street",
          "ComplexShippingAddress.Location.Lat",
          "ComplexShippingAddress.Location.Lng",
          "OwnedShippingAddress.Street",
          "OwnedShippingAddress.Location.Lat",
          "OwnedShippingAddress.Location.Lng"],
            ["Column1", "Column2", "Column3", "Season", "SeasonAsString",
          "ComplexShippingAddress.Street",
          "ComplexShippingAddress.Location.Lat",
          "ComplexShippingAddress.Location.Lng",
          "OwnedShippingAddress.Street",
          "OwnedShippingAddress.Location.Lat",
          "OwnedShippingAddress.Location.Lng"],
            new BulkMergeOptions()
            {
                LogTo = LogTo,
                ReturnDbGeneratedId = true
            });
        var result2 = await _context.BulkMergeAsync(compositeKeyRows,
            ["Id1", "Id2"],
            ["Column1", "Column2", "Column3", "Season", "SeasonAsString"],
            ["Id1", "Id2", "Column1", "Column2", "Column3", "Season", "SeasonAsString"],
            new BulkMergeOptions()
            {
                LogTo = LogTo
            });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length + insertLength, result1.AffectedRows);
        Assert.Equal(insertLength, result1.InsertedRows);
        Assert.Equal(length, result1.UpdatedRows);

        Assert.Equal(length + insertLength, result2.AffectedRows);
        Assert.Equal(insertLength, result2.InsertedRows);
        Assert.Equal(length, result2.UpdatedRows);

        for (var i = 0; i < length + insertLength; i++)
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
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task BulkMerge_ReturnDbGeneratedId_True(int length)
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

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (var i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = await _context.BulkMergeAsync(configurationEntries,
        x => x.Id,
 x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
             x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
   new BulkMergeOptions()
   {
       LogTo = LogTo
   });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length * 2, result.AffectedRows);
        Assert.Equal(length, result.InsertedRows);
        Assert.Equal(length, result.UpdatedRows);

        for (var i = 0; i < length * 2; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task BulkMerge_ReturnDbGeneratedId_False(int length)
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

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (var i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = await _context.BulkMergeAsync(configurationEntries,
             x => x.Id,
      x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
             x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
             new BulkMergeOptions()
             {
                 ReturnDbGeneratedId = false,
                 LogTo = LogTo
             });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length * 2, result.AffectedRows);
        Assert.Equal(length, result.InsertedRows);
        Assert.Equal(length, result.UpdatedRows);

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }

        for (var i = length; i < length * 2; i++)
        {
            Assert.Equal(Guid.Empty, configurationEntries[i].Id);
            Assert.NotEqual(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task BulkMerge_UpdateOnly(int length)
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

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (var i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = await _context.BulkMergeAsync(configurationEntries,
             x => x.Id,
  x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
     x => new { },
       new BulkMergeOptions()
       {
           LogTo = LogTo
       });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length, result.AffectedRows);
        Assert.Equal(0, result.InsertedRows);
        Assert.Equal(length, result.UpdatedRows);
        Assert.Equal(length, configurationEntriesInDb.Count);

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task BulkMerge_InsertOnly(int length)
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

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (var i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = await _context.BulkMergeAsync(configurationEntries,
                x => x.Id,
        x => new { },
                x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
      new BulkMergeOptions()
      {
          LogTo = LogTo
      });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length, result.AffectedRows);
        Assert.Equal(length, result.InsertedRows);
        Assert.Equal(0, result.UpdatedRows);
        Assert.Equal(length * 2, configurationEntriesInDb.Count);

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.NotEqual(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.NotEqual(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }

        for (var i = length; i < length * 2; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task BulkMerge_DoNothing(int length)
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

        foreach (var entry in configurationEntries)
        {
            entry.Description = "Updated";
            entry.UpdatedDateTime = DateTimeOffset.Now;
        }

        for (var i = length; i < length * 2; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        var result = await _context.BulkMergeAsync(configurationEntries,
    x => x.Id,
             x => new { },
             x => new { },
  new BulkMergeOptions()
  {
      LogTo = LogTo
  });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(0, result.AffectedRows);
        Assert.Equal(0, result.InsertedRows);
        Assert.Equal(0, result.UpdatedRows);
        Assert.Equal(length, configurationEntriesInDb.Count);

        for (var i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.NotEqual(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.NotEqual(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }
}