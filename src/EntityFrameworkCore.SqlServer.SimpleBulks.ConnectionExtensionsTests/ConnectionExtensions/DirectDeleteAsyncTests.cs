using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class DirectDeleteAsyncTests : BaseTest
{
    public DirectDeleteAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.DirectDelete")
    {
        var tran = _context.Database.BeginTransaction();

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < 100; i++)
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
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Winter
            });
        }

        _context.BulkInsert(rows,
                row => new { row.Column1, row.Column2, row.Column3, row.Season });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season });

        tran.Commit();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(95)]
    public async Task Direct_Delete_Using_Linq_With_Transaction(int index)
    {
        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var row = _context.SingleKeyRows.AsNoTracking().Skip(index).First();
        var compositeKeyRow = _context.CompositeKeyRows.AsNoTracking().Skip(index).First();

        var deleteResult1 = await connectionContext.DirectDeleteAsync(row,
            row => row.Id,
      options: new BulkDeleteOptions()
      {
          LogTo = _output.WriteLine
      });

        var deleteResult2 = await connectionContext.DirectDeleteAsync(compositeKeyRow,
            row => new { row.Id1, row.Id2 },
               options: new BulkDeleteOptions()
               {
                   LogTo = _output.WriteLine
               });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, deleteResult1.AffectedRows);
        Assert.Equal(1, deleteResult2.AffectedRows);
        Assert.Equal(99, dbRows.Count);
        Assert.Equal(99, dbCompositeKeyRows.Count);
        Assert.Null(dbRows.FirstOrDefault(x => x.Id == row.Id));
        Assert.Null(dbCompositeKeyRows.FirstOrDefault(x => x.Id1 == compositeKeyRow.Id1 && x.Id2 == compositeKeyRow.Id2));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(95)]
    public async Task Direct_Delete_Using_Linq_With_RolledBack_Transaction(int index)
    {
        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var row = _context.SingleKeyRows.AsNoTracking().Skip(index).First();
        var compositeKeyRow = _context.CompositeKeyRows.AsNoTracking().Skip(index).First();

        var deleteResult1 = await connectionContext.DirectDeleteAsync(row,
            row => row.Id,
               options: new BulkDeleteOptions()
               {
                   LogTo = _output.WriteLine
               });

        var deleteResult2 = await connectionContext.DirectDeleteAsync(compositeKeyRow,
            row => new { row.Id1, row.Id2 },
        options: new BulkDeleteOptions()
        {
            LogTo = _output.WriteLine
        });

        tran.Rollback();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, deleteResult1.AffectedRows);
        Assert.Equal(1, deleteResult2.AffectedRows);
        Assert.Equal(100, dbRows.Count);
        Assert.Equal(100, dbCompositeKeyRows.Count);
        Assert.NotNull(dbRows.FirstOrDefault(x => x.Id == row.Id));
        Assert.NotNull(dbCompositeKeyRows.FirstOrDefault(x => x.Id1 == compositeKeyRow.Id1 && x.Id2 == compositeKeyRow.Id2));
    }
}