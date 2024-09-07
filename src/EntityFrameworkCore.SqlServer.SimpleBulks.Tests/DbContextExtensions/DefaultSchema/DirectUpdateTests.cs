using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions.DefaultSchema
{
    public class DirectUpdateTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private TestDbContext _context;

        public DirectUpdateTests(ITestOutputHelper output)
        {
            _output = output;

            _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.DirectUpdate.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
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
        [InlineData(5)]
        [InlineData(90)]
        public void Direct_Update_Using_Linq_With_Transaction(int index)
        {
            SeedData(100);

            var tran = _context.Database.BeginTransaction();

            var rows = _context.SingleKeyRows.AsNoTracking().ToList();
            var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            var row = rows.Skip(index).First();
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;

            var compositeKeyRow = compositeKeyRows.Skip(index).First();
            compositeKeyRow.Column2 = "abc";
            compositeKeyRow.Column3 = DateTime.Now;

            var updateResult1 = _context.DirectUpdate(row,
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            var updateResult2 = _context.DirectUpdate(compositeKeyRow,
                    row => new { row.Column3, row.Column2 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

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

                Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
                Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
                Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
                Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
                Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            }
        }

        [Theory]
        [InlineData(5)]
        [InlineData(90)]
        public void Direct_Update_Using_Dynamic_String_With_Transaction(int index)
        {
            SeedData(100);

            var tran = _context.Database.BeginTransaction();

            var rows = _context.SingleKeyRows.AsNoTracking().ToList();
            var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            var row = rows.Skip(index).First();
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;

            var compositeKeyRow = compositeKeyRows.Skip(index).First();
            compositeKeyRow.Column2 = "abc";
            compositeKeyRow.Column3 = DateTime.Now;

            var updateResult1 = _context.DirectUpdate(row,
                  new[] { "Column3", "Column2" },
                  options =>
                  {
                      options.LogTo = _output.WriteLine;
                  });

            var updateResult2 = _context.DirectUpdate(compositeKeyRow,
                new[] { "Column3", "Column2" },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

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

                Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
                Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
                Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
                Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
                Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            }
        }
    }
}