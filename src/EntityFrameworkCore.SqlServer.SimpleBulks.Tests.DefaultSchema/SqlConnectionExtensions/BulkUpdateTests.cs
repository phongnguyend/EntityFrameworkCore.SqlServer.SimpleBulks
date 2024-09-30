using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.SqlConnectionExtensions
{
    public class BulkUpdateTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private TestDbContext _context;
        private SqlConnection _connection;

        public BulkUpdateTests(ITestOutputHelper output)
        {
            _output = output;

            var connectionString = $"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.BulkInsert.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False";
            _context = new TestDbContext(connectionString);
            _context.Database.EnsureCreated();

            _connection = new SqlConnection(connectionString);

            TableMapper.Register(typeof(SingleKeyRow<int>), "SingleKeyRows");
            TableMapper.Register(typeof(CompositeKeyRow<int, int>), "CompositeKeyRows");

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
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void Bulk_Update_Without_Transaction(bool useLinq, bool omitTableName)
        {
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

            if (useLinq)
            {
                if (omitTableName)
                {
                    _connection.BulkUpdate(rows,
                        row => row.Id,
                        row => new { row.Column3, row.Column2 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkUpdate(compositeKeyRows,
                        row => new { row.Id1, row.Id2 },
                        row => new { row.Column3, row.Column2 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }
                else
                {
                    _connection.BulkUpdate(rows, new TableInfor("SingleKeyRows"),
                        row => row.Id,
                        row => new { row.Column3, row.Column2 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkUpdate(compositeKeyRows, new TableInfor("CompositeKeyRows"),
                        row => new { row.Id1, row.Id2 },
                        row => new { row.Column3, row.Column2 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }

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

                if (omitTableName)
                {
                    _connection.BulkMerge(rows,
                        row => row.Id,
                        row => new { row.Column1, row.Column2 },
                        row => new { row.Column1, row.Column2, row.Column3 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkMerge(compositeKeyRows,
                        row => new { row.Id1, row.Id2 },
                        row => new { row.Column1, row.Column2, row.Column3 },
                        row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }
                else
                {
                    _connection.BulkMerge(rows, new TableInfor("SingleKeyRows"),
                        row => row.Id,
                        row => new { row.Column1, row.Column2 },
                        row => new { row.Column1, row.Column2, row.Column3 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkMerge(compositeKeyRows, new TableInfor("CompositeKeyRows"),
                        row => new { row.Id1, row.Id2 },
                        row => new { row.Column1, row.Column2, row.Column3 },
                        row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }

            }
            else
            {
                if (omitTableName)
                {
                    _connection.BulkUpdate(rows,
                        "Id",
                        new[] { "Column3", "Column2" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkUpdate(compositeKeyRows,
                        new[] { "Id1", "Id2" },
                        new[] { "Column3", "Column2" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }
                else
                {
                    _connection.BulkUpdate(rows, new TableInfor("SingleKeyRows"),
                        "Id",
                        new[] { "Column3", "Column2" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkUpdate(compositeKeyRows, new TableInfor("CompositeKeyRows"),
                        new[] { "Id1", "Id2" },
                        new[] { "Column3", "Column2" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }

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

                if (omitTableName)
                {
                    _connection.BulkMerge(rows,
                        "Id",
                        new[] { "Column1", "Column2" },
                        new[] { "Column1", "Column2", "Column3" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkMerge(compositeKeyRows,
                        new[] { "Id1", "Id2" },
                        new[] { "Column1", "Column2", "Column3" },
                        new[] { "Id1", "Id2", "Column1", "Column2", "Column3" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }
                else
                {
                    _connection.BulkMerge(rows, new TableInfor("SingleKeyRows"),
                        "Id",
                        new[] { "Column1", "Column2" },
                        new[] { "Column1", "Column2", "Column3" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });

                    _connection.BulkMerge(compositeKeyRows, new TableInfor("CompositeKeyRows"),
                        new[] { "Id1", "Id2" },
                        new[] { "Column1", "Column2", "Column3" },
                        new[] { "Id1", "Id2", "Column1", "Column2", "Column3" },
                        options =>
                        {
                            options.LogTo = _output.WriteLine;
                        });
                }
            }

            // Assert
            var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
            var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

            for (int i = 0; i < 101; i++)
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