using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class BulkUpdateTests : BaseTest
{
    private string _schema = "";

    public BulkUpdateTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkUpdate")
    {
        TableMapper.Register<SingleKeyRow<int>>(new SqlTableInfor(_schema, "SingleKeyRows"));
        TableMapper.Register<CompositeKeyRow<int, int>>(new SqlTableInfor(_schema, "CompositeKeyRows"));

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

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void Bulk_Update_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

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
                connectionContext.BulkUpdate(rows,
      row => row.Id,
        row => new { row.Column3, row.Column2 },
             options: new BulkUpdateOptions()
             {
                 LogTo = _output.WriteLine
             });

                connectionContext.BulkUpdate(compositeKeyRows,
                 row => new { row.Id1, row.Id2 },
                  row => new { row.Column3, row.Column2 },
                 options: new BulkUpdateOptions()
                 {
                     LogTo = _output.WriteLine
                 });
            }
            else
            {
                connectionContext.BulkUpdate(rows,
              row => row.Id,
              row => new { row.Column3, row.Column2 },
              new SqlTableInfor(_schema, "SingleKeyRows"),
                 options: new BulkUpdateOptions()
                 {
                     LogTo = _output.WriteLine
                 });

                connectionContext.BulkUpdate(compositeKeyRows,
                       row => new { row.Id1, row.Id2 },
                        row => new { row.Column3, row.Column2 },
                        new SqlTableInfor(_schema, "CompositeKeyRows"),
                         options: new BulkUpdateOptions()
                         {
                             LogTo = _output.WriteLine
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
                connectionContext.BulkMerge(rows,
                     row => row.Id,
                row => new { row.Column1, row.Column2 },
                    row => new { row.Column1, row.Column2, row.Column3 },
                 options: new BulkMergeOptions()
                 {
                     LogTo = _output.WriteLine
                 });

                connectionContext.BulkMerge(compositeKeyRows,
                    row => new { row.Id1, row.Id2 },
                  row => new { row.Column1, row.Column2, row.Column3 },
                 row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                  options: new BulkMergeOptions()
                  {
                      LogTo = _output.WriteLine
                  });
            }
            else
            {
                connectionContext.BulkMerge(rows,
               row => row.Id,
                        row => new { row.Column1, row.Column2 },
                  row => new { row.Column1, row.Column2, row.Column3 },
                  new SqlTableInfor(_schema, "SingleKeyRows"),
         options: new BulkMergeOptions()
         {
             LogTo = _output.WriteLine
         });

                connectionContext.BulkMerge(compositeKeyRows,
                 row => new { row.Id1, row.Id2 },
            row => new { row.Column1, row.Column2, row.Column3 },
             row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
             new SqlTableInfor(_schema, "CompositeKeyRows"),
            options: new BulkMergeOptions()
            {
                LogTo = _output.WriteLine
            });
            }

        }
        else
        {
            if (omitTableName)
            {
                connectionContext.BulkUpdate(rows,
              ["Id"],
                      ["Column3", "Column2"],
             options: new BulkUpdateOptions()
             {
                 LogTo = _output.WriteLine
             });

                connectionContext.BulkUpdate(compositeKeyRows,
                ["Id1", "Id2"],
                ["Column3", "Column2"],
      options: new BulkUpdateOptions()
      {
          LogTo = _output.WriteLine
      });
            }
            else
            {
                connectionContext.BulkUpdate(rows,
                 ["Id"],
                    ["Column3", "Column2"],
                    new SqlTableInfor(_schema, "SingleKeyRows"),
                options: new BulkUpdateOptions()
                {
                    LogTo = _output.WriteLine
                });

                connectionContext.BulkUpdate(compositeKeyRows,
                      ["Id1", "Id2"],
                            ["Column3", "Column2"],
                            new SqlTableInfor(_schema, "CompositeKeyRows"),
           options: new BulkUpdateOptions()
           {
               LogTo = _output.WriteLine
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
                connectionContext.BulkMerge(rows,
                ["Id"],
              ["Column1", "Column2"],
            ["Column1", "Column2", "Column3"],
                           options: new BulkMergeOptions()
                           {
                               LogTo = _output.WriteLine
                           });

                connectionContext.BulkMerge(compositeKeyRows,
                     ["Id1", "Id2"],
              ["Column1", "Column2", "Column3"],
                  ["Id1", "Id2", "Column1", "Column2", "Column3"],
                     options: new BulkMergeOptions()
                     {
                         LogTo = _output.WriteLine
                     });
            }
            else
            {
                connectionContext.BulkMerge(rows,
                         ["Id"],
                         ["Column1", "Column2"],
                         ["Column1", "Column2", "Column3"],
                         new SqlTableInfor(_schema, "SingleKeyRows"),
                         options: new BulkMergeOptions()
                         {
                             LogTo = _output.WriteLine
                         });

                connectionContext.BulkMerge(compositeKeyRows,
              ["Id1", "Id2"],
              ["Column1", "Column2", "Column3"],
         ["Id1", "Id2", "Column1", "Column2", "Column3"],
         table: new SqlTableInfor(_schema, "CompositeKeyRows"),
     options: new BulkMergeOptions()
     {
         LogTo = _output.WriteLine
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