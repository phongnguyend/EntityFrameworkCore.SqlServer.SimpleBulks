using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class BulkDeleteAsyncTests : BaseTest
{
    private string _schema = "";

    public BulkDeleteAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkDelete")
    {
        TableMapper.Register<SingleKeyRow<int>>(new SqlTableInfor(_schema, "SingleKeyRows"));
        TableMapper.Register<CompositeKeyRow<int, int>>(new SqlTableInfor(_schema, "CompositeKeyRows"));

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < 100; i++)
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
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Bulk_Delete_Without_Transaction(bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var rows = _context.SingleKeyRows.AsNoTracking().Take(99).ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().Take(99).ToList();

        if (useLinq)
        {
            if (omitTableName)
            {
                await connectionContext.BulkDeleteAsync(rows, row => row.Id,
                   options: new BulkDeleteOptions()
                   {
                       LogTo = _output.WriteLine
                   });
                await connectionContext.BulkDeleteAsync(compositeKeyRows, row => new { row.Id1, row.Id2 },
                    options: new BulkDeleteOptions()
                    {
                        LogTo = _output.WriteLine
                    });
            }
            else
            {
                await connectionContext.BulkDeleteAsync(rows, row => row.Id,
                    new SqlTableInfor(_schema, "SingleKeyRows"),
                    options: new BulkDeleteOptions()
                    {
                        LogTo = _output.WriteLine
                    });
                await connectionContext.BulkDeleteAsync(compositeKeyRows, row => new { row.Id1, row.Id2 },
                    new SqlTableInfor(_schema, "CompositeKeyRows"),
                    options: new BulkDeleteOptions()
                    {
                        LogTo = _output.WriteLine
                    });
            }
        }
        else
        {
            if (omitTableName)
            {
                await connectionContext.BulkDeleteAsync(rows, ["Id"],
                    options: new BulkDeleteOptions()
                    {
                        LogTo = _output.WriteLine
                    });
                await connectionContext.BulkDeleteAsync(compositeKeyRows, ["Id1", "Id2"],
                    options: new BulkDeleteOptions()
                    {
                        LogTo = _output.WriteLine
                    });
            }
            else
            {
                await connectionContext.BulkDeleteAsync(rows, ["Id"],
                    new SqlTableInfor(_schema, "SingleKeyRows"),
                    new BulkDeleteOptions()
                    {
                        LogTo = _output.WriteLine
                    });
                await connectionContext.BulkDeleteAsync(compositeKeyRows, ["Id1", "Id2"],
                     new SqlTableInfor(_schema, "CompositeKeyRows"),
                    new BulkDeleteOptions()
                    {
                        LogTo = _output.WriteLine
                    });
            }
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Single(dbRows);
        Assert.Single(dbCompositeKeyRows);
    }
}