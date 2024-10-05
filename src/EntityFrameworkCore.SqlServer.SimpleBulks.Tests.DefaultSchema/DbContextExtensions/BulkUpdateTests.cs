using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions;

public class BulkUpdateTests : IDisposable
{
    private readonly ITestOutputHelper _output;

    private TestDbContext _context;

    public BulkUpdateTests(ITestOutputHelper output)
    {
        _output = output;

        _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.BulkUpdate.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
        _context.Database.EnsureCreated();
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
                Column3 = DateTime.Now
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i + 1,
                Id2 = i + 1,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now
            });
        }

        _context.BulkInsert(rows,
                row => new { row.Column1, row.Column2, row.Column3 });

        _context.BulkInsert(compositeKeyRows,
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

        tran.Commit();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
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
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        var updateResult1 = _context.BulkUpdate(rows,
                row => new { row.Column3, row.Column2 },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        var updateResult2 = _context.BulkUpdate(compositeKeyRows,
                row => new { row.Column3, row.Column2 },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length + 1,
            Column2 = "Inserted using Merge" + length + 1,
            Column3 = DateTime.Now,
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
        });

        _context.BulkMerge(rows,
                row => row.Id,
                row => new { row.Column1, row.Column2 },
                row => new { row.Column1, row.Column2, row.Column3 },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

        _context.BulkMerge(compositeKeyRows,
                row => new { row.Id1, row.Id2 },
                row => new { row.Column1, row.Column2, row.Column3 },
                row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
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

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
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
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        var updateResult1 = _context.BulkUpdate(rows,
              [ "Column3", "Column2" ],
              options =>
              {
                  options.LogTo = _output.WriteLine;
              });

        var updateResult2 = _context.BulkUpdate(compositeKeyRows,
            [ "Column3", "Column2" ],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        rows.Add(new SingleKeyRow<int>
        {
            Column1 = length + 1,
            Column2 = "Inserted using Merge" + length + 1,
            Column3 = DateTime.Now,
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
        });

        _context.BulkMerge(rows,
            "Id",
            [ "Column1", "Column2" ],
            [ "Column1", "Column2", "Column3" ],
            options =>
            {
                options.LogTo = _output.WriteLine;
            });
        _context.BulkMerge(compositeKeyRows,
            [ "Id1", "Id2" ],
            [ "Column1", "Column2", "Column3" ],
            [ "Id1", "Id2", "Column1", "Column2", "Column3" ],
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

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
        }
    }
}