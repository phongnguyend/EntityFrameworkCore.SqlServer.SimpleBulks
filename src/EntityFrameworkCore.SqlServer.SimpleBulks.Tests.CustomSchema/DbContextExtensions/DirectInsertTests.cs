using EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions
{
    public class DirectInsertTests : IDisposable
    {

        private TestDbContext _context;
        private readonly ITestOutputHelper _output;

        public DirectInsertTests(ITestOutputHelper output)
        {
            _output = output;
            _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.DirectInsert.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        [Fact]
        public void Direct_Insert_Using_Linq_Without_Transaction()
        {
            var row = new SingleKeyRow<int>
            {
                Column1 = 1,
                Column2 = "" + 1,
                Column3 = DateTime.Now
            };

            var compositeKeyRow = new CompositeKeyRow<int, int>
            {
                Id1 = 1,
                Id2 = 1,
                Column1 = 1,
                Column2 = "" + 1,
                Column3 = DateTime.Now
            };

            _context.DirectInsert(row,
                    row => new { row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            _context.DirectInsert(compositeKeyRow,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });


            // Assert
            var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
            var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            Assert.Single(dbRows);
            Assert.Single(dbCompositeKeyRows);

            Assert.Equal(row.Id, dbRows[0].Id);
            Assert.Equal(row.Column1, dbRows[0].Column1);
            Assert.Equal(row.Column2, dbRows[0].Column2);
            Assert.Equal(row.Column3, dbRows[0].Column3);

            Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
            Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
            Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
            Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
            Assert.Equal(compositeKeyRow.Column3, dbCompositeKeyRows[0].Column3);
        }

        [Fact]
        public void Direct_Insert_Using_Linq_With_Transaction_Committed()
        {
            var tran = _context.Database.BeginTransaction();

            var row = new SingleKeyRow<int>
            {
                Column1 = 1,
                Column2 = "" + 1,
                Column3 = DateTime.Now
            };

            var compositeKeyRow = new CompositeKeyRow<int, int>
            {
                Id1 = 1,
                Id2 = 1,
                Column1 = 1,
                Column2 = "" + 1,
                Column3 = DateTime.Now
            };

            _context.DirectInsert(row,
                    row => new { row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            _context.DirectInsert(compositeKeyRow,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            tran.Commit();

            // Assert
            var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
            var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            Assert.Single(dbRows);
            Assert.Single(dbCompositeKeyRows);

            Assert.Equal(row.Id, dbRows[0].Id);
            Assert.Equal(row.Column1, dbRows[0].Column1);
            Assert.Equal(row.Column2, dbRows[0].Column2);
            Assert.Equal(row.Column3, dbRows[0].Column3);

            Assert.Equal(compositeKeyRow.Id1, dbCompositeKeyRows[0].Id1);
            Assert.Equal(compositeKeyRow.Id2, dbCompositeKeyRows[0].Id2);
            Assert.Equal(compositeKeyRow.Column1, dbCompositeKeyRows[0].Column1);
            Assert.Equal(compositeKeyRow.Column2, dbCompositeKeyRows[0].Column2);
            Assert.Equal(compositeKeyRow.Column3, dbCompositeKeyRows[0].Column3);
        }

        [Fact]
        public void Direct_Insert_Using_Linq_With_Transaction_RolledBack()
        {
            var tran = _context.Database.BeginTransaction();

            var row = new SingleKeyRow<int>
            {
                Column1 = 1,
                Column2 = "" + 1,
                Column3 = DateTime.Now
            };

            var compositeKeyRow = new CompositeKeyRow<int, int>
            {
                Id1 = 1,
                Id2 = 1,
                Column1 = 1,
                Column2 = "" + 1,
                Column3 = DateTime.Now
            };

            _context.DirectInsert(row,
                    row => new { row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            _context.DirectInsert(compositeKeyRow,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                    options =>
                    {
                        options.LogTo = _output.WriteLine;
                    });

            tran.Rollback();

            // Assert
            var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
            var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            Assert.Empty(dbRows);
            Assert.Empty(dbCompositeKeyRows);
        }

        [Fact]
        public void Direct_Insert_KeepIdentity()
        {
            var configurationEntry = new ConfigurationEntry
            {
                Id = Guid.NewGuid(),
                Key = $"Key1",
                Value = $"Value1",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            };

            _context.DirectInsert(configurationEntry, options =>
            {
                options.KeepIdentity = true;
                options.LogTo = _output.WriteLine;
            });

            // Assert
            var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
            Assert.Single(configurationEntriesInDb);
            Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
            Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
            Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
            Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
            Assert.Equal(configurationEntry.CreatedDateTime, configurationEntriesInDb[0].CreatedDateTime);
        }

        [Fact]
        public void Direct_Insert_Return_DbGeneratedId()
        {
            var configurationEntry = new ConfigurationEntry
            {
                Key = $"Key1",
                Value = $"Value1",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            };

            _context.DirectInsert(configurationEntry, options =>
            {
                options.LogTo = _output.WriteLine;
            });

            // Assert
            var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();
            Assert.Single(configurationEntriesInDb);
            Assert.NotEqual(Guid.Empty, configurationEntriesInDb[0].Id);
            Assert.Equal(configurationEntry.Id, configurationEntriesInDb[0].Id);
            Assert.Equal(configurationEntry.Key, configurationEntriesInDb[0].Key);
            Assert.Equal(configurationEntry.Value, configurationEntriesInDb[0].Value);
            Assert.Equal(configurationEntry.Description, configurationEntriesInDb[0].Description);
            Assert.Equal(configurationEntry.CreatedDateTime, configurationEntriesInDb[0].CreatedDateTime);
        }
    }
}