using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.DbContextExtensions;

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

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(5)]
    [InlineData(95)]
    public async Task DirectDelete_PrimaryKeys_With_Transaction(int index)
    {
        var tran = _context.Database.BeginTransaction();

        var row = _context.SingleKeyRows.AsNoTracking().Skip(index).First();
        var compositeKeyRow = _context.CompositeKeyRows.AsNoTracking().Skip(index).First();

        var options = new BulkDeleteOptions()
        {
            LogTo = LogTo
        };

        var deleteResult1 = await _context.DirectDeleteAsync(row, options);

        var deleteResult2 = await _context.DirectDeleteAsync(compositeKeyRow, options);

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
    public async Task DirectDelete_PrimaryKeys_With_RolledBack_Transaction(int index)
    {
        var tran = _context.Database.BeginTransaction();

        var row = _context.SingleKeyRows.AsNoTracking().Skip(index).First();
        var compositeKeyRow = _context.CompositeKeyRows.AsNoTracking().Skip(index).First();

        var options = new BulkDeleteOptions()
        {
            LogTo = LogTo
        };

        var deleteResult1 = await _context.DirectDeleteAsync(row, options);

        var deleteResult2 = await _context.DirectDeleteAsync(compositeKeyRow, options);

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

    [Theory]
    [InlineData(5)]
    [InlineData(95)]
    public async Task DirectDelete_SpecifiedKeys(int index)
    {
        var tran = _context.Database.BeginTransaction();

        var row = _context.SingleKeyRows.AsNoTracking().Skip(index).First();
        var compositeKeyRow = _context.CompositeKeyRows.AsNoTracking().Skip(index).First();

        var options = new BulkDeleteOptions()
        {
            LogTo = LogTo
        };

        var deleteResult1 = await _context.DirectDeleteAsync(row, x => x.Id, options);

        var deleteResult2 = await _context.DirectDeleteAsync(compositeKeyRow, x => new { x.Id1, x.Id2 }, options);

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
    public async Task DirectDelete_SpecifiedKeys_DynamicString(int index)
    {
        var tran = _context.Database.BeginTransaction();

        var row = _context.SingleKeyRows.AsNoTracking().Skip(index).First();
        var compositeKeyRow = _context.CompositeKeyRows.AsNoTracking().Skip(index).First();

        var options = new BulkDeleteOptions()
        {
            LogTo = LogTo
        };

        var deleteResult1 = await _context.DirectDeleteAsync(row, ["Id"], options);

        var deleteResult2 = await _context.DirectDeleteAsync(compositeKeyRow, ["Id1", "Id2"], options);

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
}