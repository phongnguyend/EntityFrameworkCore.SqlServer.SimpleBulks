using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("SqlServerCollection")]
public class BulkUpdateAsyncTests : BaseTest
{
    public BulkUpdateAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkUpdate")
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
                Id1 = i + 1,
                Id2 = i + 1,
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
    [InlineData(1)]
    [InlineData(100)]
    public async Task Bulk_Update_Using_Linq_With_Transaction(int length)
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

        var updateResult1 = await _context.BulkUpdateAsync(rows,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
                new BulkUpdateOptions()
                {
                    LogTo = LogTo
                });

        var updateResult2 = await _context.BulkUpdateAsync(compositeKeyRows,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
                new BulkUpdateOptions()
                {
                    LogTo = LogTo
                });

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length + 1,
            Column2 = "Inserted using Merge" + length + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street " + length + 1,
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128 + length + 1,
                    Lng = -74.0060 - length + 1
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street " + length + 1,
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128 + length + 1,
                    Lng = -74.0060 - length + 1
                }
            }
        });

        var newId1 = length + 1;
        var newId2 = length + 1;

        compositeKeyRows.Add(new CompositeKeyRow<int, int>
        {
            Id1 = newId1,
            Id2 = newId2,
            Column1 = newId2,
            Column2 = "Inserted using Merge" + newId2,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn
        });

        await _context.BulkMergeAsync(rows,
                row => row.Id,
                row => new
                {
                    row.Column1,
                    row.Column2,
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
                    LogTo = LogTo
                });

        await _context.BulkMergeAsync(compositeKeyRows,
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

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (var i = 0; i < length + 1; i++)
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
    public async Task Bulk_Update_Using_Dynamic_String_With_Transaction(int length)
    {
        await SeedData(length);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Summer;
            row.SeasonAsString = Season.Summer;
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.Season = Season.Summer;
            row.SeasonAsString = Season.Summer;
        }

        var updateResult1 = await _context.BulkUpdateAsync(rows,
              ["Column3", "Column2", "Season", "SeasonAsString"],
              new BulkUpdateOptions()
              {
                  LogTo = LogTo
              });

        var updateResult2 = await _context.BulkUpdateAsync(compositeKeyRows,
            ["Column3", "Column2", "Season", "SeasonAsString"],
            new BulkUpdateOptions()
            {
                LogTo = LogTo
            });

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length + 1,
            Column2 = "Inserted using Merge" + length + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn,
            ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Street " + length + 1,
                Location = new ComplexTypeLocation
                {
                    Lat = 40.7128 + length + 1,
                    Lng = -74.0060 - length + 1
                }
            },
            OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Street " + length + 1,
                Location = new OwnedTypeLocation
                {
                    Lat = 40.7128 + length + 1,
                    Lng = -74.0060 - length + 1
                }
            }
        });

        var newId1 = length + 1;
        var newId2 = length + 1;

        compositeKeyRows.Add(new CompositeKeyRow<int, int>
        {
            Id1 = newId1,
            Id2 = newId2,
            Column1 = newId2,
            Column2 = "Inserted using Merge" + newId2,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn
        });

        await _context.BulkMergeAsync(rows,
            ["Id"],
            ["Column1", "Column2", "Season", "SeasonAsString",
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
                LogTo = LogTo
            });
        await _context.BulkMergeAsync(compositeKeyRows,
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

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (var i = 0; i < length + 1; i++)
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
}