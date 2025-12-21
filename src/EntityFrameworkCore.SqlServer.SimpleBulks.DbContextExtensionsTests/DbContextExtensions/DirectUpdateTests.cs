using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("SqlServerCollection")]
public class DirectUpdateTests : BaseTest
{
    public DirectUpdateTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.DirectUpdate")
    {
    }

    private void SeedData(int length)
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (int i = 0; i < length; i++)
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

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(90)]
    public void DirectUpdate_PrimaryKeys(int index)
    {
        SeedData(100);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Spring;
        row.SeasonAsString = Season.Spring;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Spring;
        compositeKeyRow.SeasonAsString = Season.Spring;

        var options = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        var updateResult1 = _context.DirectUpdate(row,
            row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
            options);

        var updateResult2 = _context.DirectUpdate(compositeKeyRow,
            row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
            options);

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (int i = 0; i < 100; i++)
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
    [InlineData(5)]
    [InlineData(90)]
    public void DirectUpdate_PrimaryKeys_DynamicString(int index)
    {
        SeedData(100);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Summer;
        row.SeasonAsString = Season.Summer;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Summer;
        compositeKeyRow.SeasonAsString = Season.Summer;

        var options = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        var updateResult1 = _context.DirectUpdate(row,
            ["Column3", "Column2", "Season", "SeasonAsString"],
            options);

        var updateResult2 = _context.DirectUpdate(compositeKeyRow,
            ["Column3", "Column2", "Season", "SeasonAsString"],
            options);

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (int i = 0; i < 100; i++)
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
    [InlineData(5)]
    [InlineData(90)]
    public void DirectUpdate_SpecifiedKeys(int index)
    {
        SeedData(100);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Spring;
        row.SeasonAsString = Season.Spring;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Spring;
        compositeKeyRow.SeasonAsString = Season.Spring;

        var options = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        var updateResult1 = _context.DirectUpdate(row, x => x.Id,
            row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
            options);

        var updateResult2 = _context.DirectUpdate(compositeKeyRow, x => new { x.Id1, x.Id2 },
            row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
            options);

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (int i = 0; i < 100; i++)
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
    [InlineData(5)]
    [InlineData(90)]
    public void DirectUpdate_SpecifiedKeys_DynamicString(int index)
    {
        SeedData(100);

        var tran = _context.Database.BeginTransaction();

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Summer;
        row.SeasonAsString = Season.Summer;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Summer;
        compositeKeyRow.SeasonAsString = Season.Summer;

        var options = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        var updateResult1 = _context.DirectUpdate(row, ["Id"],
            ["Column3", "Column2", "Season", "SeasonAsString"],
            options);

        var updateResult2 = _context.DirectUpdate(compositeKeyRow, ["Id1", "Id2"],
            ["Column3", "Column2", "Season", "SeasonAsString"],
            options);

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (int i = 0; i < 100; i++)
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