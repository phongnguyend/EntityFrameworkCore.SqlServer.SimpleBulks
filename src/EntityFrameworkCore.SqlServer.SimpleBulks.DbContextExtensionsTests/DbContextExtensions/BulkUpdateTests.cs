using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("SqlServerCollection")]
public class BulkUpdateTests : BaseTest
{
    public BulkUpdateTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkUpdate")
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

        _context.BulkInsert(rows,
      row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString });

        tran.Commit();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void Bulk_Update_Using_Linq_With_Transaction(int length)
    {
        SeedData(length);

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

        var updateResult1 = _context.BulkUpdate(rows,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
             new BulkUpdateOptions()
             {
                 LogTo = _output.WriteLine
             });

        var updateResult2 = _context.BulkUpdate(compositeKeyRows,
          row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
    new BulkUpdateOptions()
    {
        LogTo = _output.WriteLine
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

        _context.BulkMerge(rows,
                    row => row.Id,
          row => new { row.Column1, row.Column2, row.Season, row.SeasonAsString },
            row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
              new BulkMergeOptions()
              {
                  LogTo = _output.WriteLine
              });

        _context.BulkMerge(compositeKeyRows,
  row => new { row.Id1, row.Id2 },
           row => new { row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
 row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season, row.SeasonAsString },
         new BulkMergeOptions()
         {
             LogTo = _output.WriteLine
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
    public void Bulk_Update_Using_Dynamic_String_With_Transaction(int length)
    {
        SeedData(length);

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

        var updateResult1 = _context.BulkUpdate(rows,
              ["Column3", "Column2", "Season", "SeasonAsString"],
    new BulkUpdateOptions()
    {
        LogTo = _output.WriteLine
    });

        var updateResult2 = _context.BulkUpdate(compositeKeyRows,
            ["Column3", "Column2", "Season", "SeasonAsString"],
            new BulkUpdateOptions()
            {
                LogTo = _output.WriteLine
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

        _context.BulkMerge(rows,
       ["Id"],
        ["Column1", "Column2", "Season", "SeasonAsString"],
            ["Column1", "Column2", "Column3", "Season", "SeasonAsString"],
 new BulkMergeOptions()
 {
     LogTo = _output.WriteLine
 });
        _context.BulkMerge(compositeKeyRows,
      ["Id1", "Id2"],
       ["Column1", "Column2", "Column3", "Season", "SeasonAsString"],
       ["Id1", "Id2", "Column1", "Column2", "Column3", "Season", "SeasonAsString"],
          new BulkMergeOptions()
          {
              LogTo = _output.WriteLine
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