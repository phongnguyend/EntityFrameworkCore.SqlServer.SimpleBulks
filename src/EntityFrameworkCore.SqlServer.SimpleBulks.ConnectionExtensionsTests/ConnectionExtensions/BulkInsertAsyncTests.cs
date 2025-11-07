using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class BulkInsertAsyncTests : BaseTest
{
    private string _schema = "";

    public BulkInsertAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkInsert")
    {
    }

    [Theory]
    [InlineData(1, true, true)]
    [InlineData(1, true, false)]
    [InlineData(1, false, true)]
    [InlineData(1, false, false)]
    [InlineData(100, true, true)]
    [InlineData(100, true, false)]
    [InlineData(100, false, true)]
    [InlineData(100, false, false)]
    public async Task Bulk_Insert_Without_Transaction(int length, bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var rows = new List<SingleKeyRow<int>>();
        var compositeKeyRows = new List<CompositeKeyRow<int, int>>();

        for (var i = 0; i < length; i++)
        {
            rows.Add(new SingleKeyRow<int>
            {
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn,
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Autumn,
            });
        }

        var options = new BulkInsertOptions
        {
            LogTo = _output.WriteLine
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                await connectionContext.BulkInsertAsync(rows,
                      row => new
                      {
                          row.Column1,
                          row.Column2,
                          row.Column3,
                          row.Season,
                          row.NullableBool,
                          row.NullableDateTime,
                          row.NullableDateTimeOffset,
                          row.NullableDecimal,
                          row.NullableDouble,
                          row.NullableGuid,
                          row.NullableShort,
                          row.NullableInt,
                          row.NullableLong,
                          row.NullableFloat,
                          row.NullableString
                      },
                      options: options);

                await connectionContext.BulkInsertAsync(compositeKeyRows,
                      row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                      options: options);
            }
            else
            {
                await connectionContext.BulkInsertAsync(rows,
                    row => new
                    {
                        row.Column1,
                        row.Column2,
                        row.Column3,
                        row.Season,
                        row.NullableBool,
                        row.NullableDateTime,
                        row.NullableDateTimeOffset,
                        row.NullableDecimal,
                        row.NullableDouble,
                        row.NullableGuid,
                        row.NullableShort,
                        row.NullableInt,
                        row.NullableLong,
                        row.NullableFloat,
                        row.NullableString
                    },
                     new SqlTableInfor(_schema, "SingleKeyRows")
                     {
                         OutputId = new OutputId
                         {
                             Name = "Id",
                             Mode = OutputIdMode.ServerGenerated,
                         }
                     },
                     options: options);

                await connectionContext.BulkInsertAsync(compositeKeyRows,
                      row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
                      new SqlTableInfor(_schema, "CompositeKeyRows"),
                      options: options);
            }

        }
        else
        {
            if (omitTableName)
            {
                await connectionContext.BulkInsertAsync(rows,
                     ["Column1", "Column2", "Column3"],
                     options: options);

                await connectionContext.BulkInsertAsync(compositeKeyRows,
                     ["Id1", "Id2", "Column1", "Column2", "Column3"],
                     options: options);
            }
            else
            {
                await connectionContext.BulkInsertAsync(rows,
                      ["Column1", "Column2", "Column3"],
                      new SqlTableInfor(_schema, "SingleKeyRows")
                      {
                          OutputId = new OutputId
                          {
                              Name = "Id",
                              Mode = OutputIdMode.ServerGenerated,
                          }
                      },
                      options: options);

                await connectionContext.BulkInsertAsync(compositeKeyRows,
                      ["Id1", "Id2", "Column1", "Column2", "Column3"],
                       new SqlTableInfor(_schema, "CompositeKeyRows"),
                      options: options);
            }

        }


        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (var i = 0; i < length; i++)
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