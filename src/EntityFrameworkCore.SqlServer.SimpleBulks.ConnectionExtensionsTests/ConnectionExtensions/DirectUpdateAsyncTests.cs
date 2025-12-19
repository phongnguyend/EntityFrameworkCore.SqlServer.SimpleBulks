using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

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
                Season = Season.Winter
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i + 1,
                Id2 = i + 1,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Winter
            });
        }

        await _context.BulkInsertAsync(rows);

        await _context.BulkInsertAsync(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(90)]
    public async Task Direct_Update_Using_Linq_With_Transaction(int index)
    {
        await SeedData(100);

        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Spring;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Spring;

        var updateResult1 = await connectionContext.DirectUpdateAsync(row,
              row => new { row.Column3, row.Column2, row.Season },
           options: new BulkUpdateOptions()
           {
               LogTo = LogTo
           });

        var updateResult2 = await connectionContext.DirectUpdateAsync(compositeKeyRow,
   row => new { row.Column3, row.Column2, row.Season },
  options: new BulkUpdateOptions()
  {
      LogTo = LogTo
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

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
        }
    }

    [Theory]
    [InlineData(5)]
    [InlineData(90)]
    public async Task Direct_Update_Using_Dynamic_String_With_Transaction(int index)
    {
        await SeedData(100);

        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var row = rows.Skip(index).First();
        row.Column2 = "abc";
        row.Column3 = DateTime.Now;
        row.Season = Season.Summer;

        var compositeKeyRow = compositeKeyRows.Skip(index).First();
        compositeKeyRow.Column2 = "abc";
        compositeKeyRow.Column3 = DateTime.Now;
        compositeKeyRow.Season = Season.Summer;

        var updateResult1 = await connectionContext.DirectUpdateAsync(row,
       ["Column3", "Column2", "Season"],
options: new BulkUpdateOptions()
{
    LogTo = LogTo
});

        var updateResult2 = await connectionContext.DirectUpdateAsync(compositeKeyRow,
     ["Column3", "Column2", "Season"],
      options: new BulkUpdateOptions()
      {
          LogTo = LogTo
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

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
        }
    }
}