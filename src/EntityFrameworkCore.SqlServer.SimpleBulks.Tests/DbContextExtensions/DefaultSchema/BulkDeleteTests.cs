using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions.DefaultSchema
{
    public class BulkDeleteTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private TestDbContext _context;

        public BulkDeleteTests(ITestOutputHelper output)
        {
            _output = output;

            _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.BulkDelete.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#");
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

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void Bulk_Delete_Using_Linq_With_Transaction(int length)
        {
            var tran = _context.Database.BeginTransaction();

            var rows = _context.SingleKeyRows.AsNoTracking().Take(length).ToList();
            var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(length).ToList();

            _context.BulkDelete(rows,
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });
            _context.BulkDelete(compositeKeyRows,
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            tran.Commit();

            // Assert
            var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
            var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            Assert.Equal(100 - length, dbRows.Count);
            Assert.Equal(100 - length, dbCompositeKeyRows.Count);
        }
    }
}