using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions.DefaultSchema
{
    public class BulkUpdateTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private TestDbContext _context;

        public BulkUpdateTests(ITestOutputHelper output)
        {
            _output = output;

            _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.BulkUpdate.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#");
            _context.Database.EnsureCreated();

            var tran = _context.Database.BeginTransaction();

            var rows = new List<SingleKeyRow<int>>();
            var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

            for (int i = 0; i < 100; i++)
            {
                rows.Add(new SingleKeyRow<int>
                {
                    Column1 = i,
                    Column2 = "" + i,
                    Column3 = DateTime.Now
                });

                compositeKeyRows.Add(new CompositeKeyRow<int, int>
                {
                    Id1 = i,
                    Id2 = i,
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

        [Fact]
        public void Bulk_Update_Using_Linq_With_Transaction()
        {
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

            _context.BulkUpdate(rows,
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            _context.BulkUpdate(compositeKeyRows,
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            var newId = rows.Max(x => x.Id) + 1;

            rows.Add(new SingleKeyRow<int>
            {
                Id = newId,
                Column1 = newId,
                Column2 = "Inserted using Merge" + newId,
                Column3 = DateTime.Now,
            });

            var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
            var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

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
        }

        [Fact]
        public void Bulk_Update_Using_Dynamic_String_With_Transaction()
        {
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

            _context.BulkUpdate(rows,
                new[] { "Column3", "Column2" },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
            _context.BulkUpdate(compositeKeyRows,
                new[] { "Column3", "Column2" },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

            var newId = rows.Max(x => x.Id) + 1;

            rows.Add(new SingleKeyRow<int>
            {
                Id = newId,
                Column1 = newId,
                Column2 = "Inserted using Merge" + newId,
                Column3 = DateTime.Now,
            });

            var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
            var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

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
                new[] { "Column1", "Column2" },
                new[] { "Column1", "Column2", "Column3" },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });
            _context.BulkMerge(compositeKeyRows,
                new[] { "Id1", "Id2" },
                new[] { "Column1", "Column2", "Column3" },
                new[] { "Id1", "Id2", "Column1", "Column2", "Column3" },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

            tran.Commit();
        }
    }
}