using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

[Collection("SqlServerCollection")]
public class DirectUpdateAsyncTests : BaseTest
{
    public DirectUpdateAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.DirectUpdate")
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
    [InlineData(5)]
    [InlineData(90)]
    public async Task Direct_Update_Using_Linq_With_Transaction(int index)
    {
        await SeedData(100);

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

        var updateResult1 = await _context.DirectUpdateAsync(row,
                row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
        new BulkUpdateOptions()
        {
            LogTo = _output.WriteLine
        });

        var updateResult2 = await _context.DirectUpdateAsync(compositeKeyRow,
     row => new { row.Column3, row.Column2, row.Season, row.SeasonAsString },
         new BulkUpdateOptions()
         {
             LogTo = _output.WriteLine
         });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (var i = 0; i < 100; i++)
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
    [InlineData(5)]
    [InlineData(90)]
    public async Task Direct_Update_Using_Dynamic_String_With_Transaction(int index)
    {
        await SeedData(100);

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

        var updateResult1 = await _context.DirectUpdateAsync(row,
          ["Column3", "Column2", "Season", "SeasonAsString"],
          new BulkUpdateOptions()
          {
              LogTo = _output.WriteLine
          });

        var updateResult2 = await _context.DirectUpdateAsync(compositeKeyRow,
     ["Column3", "Column2", "Season", "SeasonAsString"],
      new BulkUpdateOptions()
      {
          LogTo = _output.WriteLine
      });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, updateResult1.AffectedRows);
        Assert.Equal(1, updateResult2.AffectedRows);

        for (var i = 0; i < 100; i++)
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