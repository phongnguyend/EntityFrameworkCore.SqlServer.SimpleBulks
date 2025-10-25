using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextAsyncExtensions;

[Collection("SqlServerCollection")]
public class BulkUpdateTests : BaseTest
{
    public BulkUpdateTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkUpdate")
    {
    }

    private async Task SeedData(int length)
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
                SeasonAsString = Season.Winter
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

        await _context.BulkInsertAsync(rows,
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

        await _context.BulkInsertAsync(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

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
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        var updateResult2 = await _context.BulkUpdateAsync(compositeKeyRows,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length + 1,
            Column2 = "Inserted using Merge" + length + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn
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
                row => new { row.Column1, row.Column2, row.Season, row.SeasonAsString },
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        await _context.BulkMergeAsync(compositeKeyRows,
                row => new { row.Id1, row.Id2 },
                row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (int i = 0; i < length + 1; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);

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
              options =>
              {
                  options.LogTo = _output.WriteLine;
              });

        var updateResult2 = await _context.BulkUpdateAsync(compositeKeyRows,
            ["Column3", "Column2", "Season", "SeasonAsString"],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length + 1,
            Column2 = "Inserted using Merge" + length + 1,
            Column3 = DateTime.Now,
            Season = Season.Autumn,
            SeasonAsString = Season.Autumn
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
            "Id",
            ["Column1", "Column2", "Season", "SeasonAsString"],
            ["Column1", "Column2", "Column3", "Season", "SeasonAsString"],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });
        await _context.BulkMergeAsync(compositeKeyRows,
            ["Id1", "Id2"],
            ["Column1", "Column2", "Column3", "Season", "SeasonAsString"],
            ["Id1", "Id2", "Column1", "Column2", "Column3", "Season", "SeasonAsString"],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(length, updateResult1.AffectedRows);
        Assert.Equal(length, updateResult2.AffectedRows);

        for (int i = 0; i < length + 1; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);
            Assert.Equal(rows[i].SeasonAsString, dbRows[i].SeasonAsString);

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