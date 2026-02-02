using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class BulkUpdateAsyncTests : BaseTest
{
    public BulkUpdateAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkUpdate")
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
                Season = Season.Autumn,
                SeasonAsString = Season.Autumn,
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
                Season = Season.Autumn,
                SeasonAsString = Season.Autumn
            });
        }

        _context.BulkInsert(rows);

        _context.BulkInsert(compositeKeyRows);

        tran.Commit();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task BulkUpdate_PrimaryKeys(bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Updated Street",
                Location = new ComplexTypeLocation
                {
                    Lat = 50.0,
                    Lng = -80.0
                }
            };
            row.OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Updated Street",
                Location = new OwnedTypeLocation
                {
                    Lat = 50.0,
                    Lng = -80.0
                }
            };
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        var updateOptions = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                await connectionContext.BulkUpdateAsync(rows,
                    row => new
                    {
                        row.Column3,
                        row.Column2,
                        row.ComplexShippingAddress.Street,
                        row.ComplexShippingAddress.Location.Lat,
                        row.ComplexShippingAddress.Location.Lng,
                        a = row.OwnedShippingAddress.Street,
                        b = row.OwnedShippingAddress.Location.Lat,
                        c = row.OwnedShippingAddress.Location.Lng
                    },
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows,
                    row => new { row.Column3, row.Column2 },
                    options: updateOptions);
            }
            else
            {
                await connectionContext.BulkUpdateAsync(rows,
                    row => new
                    {
                        row.Column3,
                        row.Column2,
                        row.ComplexShippingAddress.Street,
                        row.ComplexShippingAddress.Location.Lat,
                        row.ComplexShippingAddress.Location.Lng,
                        a = row.OwnedShippingAddress.Street,
                        b = row.OwnedShippingAddress.Location.Lat,
                        c = row.OwnedShippingAddress.Location.Lng
                    },
                    _singleKeyRowTableInfor,
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows,
                    row => new { row.Column3, row.Column2 },
                    _compositeKeyRowTableInfor,
                    options: updateOptions);
            }
        }
        else
        {
            if (omitTableName)
            {
                await connectionContext.BulkUpdateAsync(rows,
                    [
                        "Column3",
                        "Column2",
                        "ComplexShippingAddress.Street",
                        "ComplexShippingAddress.Location.Lat",
                        "ComplexShippingAddress.Location.Lng",
                        "OwnedShippingAddress.Street",
                        "OwnedShippingAddress.Location.Lat",
                        "OwnedShippingAddress.Location.Lng"
                    ],
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows,
                    ["Column3", "Column2"],
                    options: updateOptions);
            }
            else
            {
                await connectionContext.BulkUpdateAsync(rows,
                    [
                        "Column3",
                        "Column2",
                        "ComplexShippingAddress.Street",
                        "ComplexShippingAddress.Location.Lat",
                        "ComplexShippingAddress.Location.Lng",
                        "OwnedShippingAddress.Street",
                        "OwnedShippingAddress.Location.Lat",
                        "OwnedShippingAddress.Location.Lng"
                    ],
                    _singleKeyRowTableInfor,
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows,
                    ["Column3", "Column2"],
                    _compositeKeyRowTableInfor,
                    options: updateOptions);
            }
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].ComplexShippingAddress?.Street, dbRows[i].ComplexShippingAddress?.Street);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lat, dbRows[i].ComplexShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lng, dbRows[i].ComplexShippingAddress?.Location?.Lng);
            Assert.Equal(rows[i].OwnedShippingAddress?.Street, dbRows[i].OwnedShippingAddress?.Street);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lat, dbRows[i].OwnedShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lng, dbRows[i].OwnedShippingAddress?.Location?.Lng);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task BulkUpdate_SpecifiedKeys(bool useLinq, bool omitTableName)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        foreach (var row in rows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
            row.ComplexShippingAddress = new ComplexTypeAddress
            {
                Street = "Updated Street",
                Location = new ComplexTypeLocation
                {
                    Lat = 50.0,
                    Lng = -80.0
                }
            };
            row.OwnedShippingAddress = new OwnedTypeAddress
            {
                Street = "Updated Street",
                Location = new OwnedTypeLocation
                {
                    Lat = 50.0,
                    Lng = -80.0
                }
            };
        }

        foreach (var row in compositeKeyRows)
        {
            row.Column2 = "abc";
            row.Column3 = DateTime.Now;
        }

        var updateOptions = new BulkUpdateOptions()
        {
            LogTo = LogTo
        };

        if (useLinq)
        {
            if (omitTableName)
            {
                await connectionContext.BulkUpdateAsync(rows, x => x.Id,
                    row => new
                    {
                        row.Column3,
                        row.Column2,
                        row.ComplexShippingAddress.Street,
                        row.ComplexShippingAddress.Location.Lat,
                        row.ComplexShippingAddress.Location.Lng,
                        a = row.OwnedShippingAddress.Street,
                        b = row.OwnedShippingAddress.Location.Lat,
                        c = row.OwnedShippingAddress.Location.Lng
                    },
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows, x => new { x.Id1, x.Id2 },
                    row => new { row.Column3, row.Column2 },
                    options: updateOptions);
            }
            else
            {
                await connectionContext.BulkUpdateAsync(rows, x => x.Id,
                    row => new
                    {
                        row.Column3,
                        row.Column2,
                        row.ComplexShippingAddress.Street,
                        row.ComplexShippingAddress.Location.Lat,
                        row.ComplexShippingAddress.Location.Lng,
                        a = row.OwnedShippingAddress.Street,
                        b = row.OwnedShippingAddress.Location.Lat,
                        c = row.OwnedShippingAddress.Location.Lng
                    },
                    _singleKeyRowTableInfor,
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows, x => new { x.Id1, x.Id2 },
                    row => new { row.Column3, row.Column2 },
                    _compositeKeyRowTableInfor,
                    options: updateOptions);
            }
        }
        else
        {
            if (omitTableName)
            {
                await connectionContext.BulkUpdateAsync(rows, ["Id"],
                    [
                        "Column3",
                        "Column2",
                        "ComplexShippingAddress.Street",
                        "ComplexShippingAddress.Location.Lat",
                        "ComplexShippingAddress.Location.Lng",
                        "OwnedShippingAddress.Street",
                        "OwnedShippingAddress.Location.Lat",
                        "OwnedShippingAddress.Location.Lng"
                    ],
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows, ["Id1", "Id2"],
                    ["Column3", "Column2"],
                    options: updateOptions);
            }
            else
            {
                await connectionContext.BulkUpdateAsync(rows, ["Id"],
                    [
                        "Column3",
                        "Column2",
                        "ComplexShippingAddress.Street",
                        "ComplexShippingAddress.Location.Lat",
                        "ComplexShippingAddress.Location.Lng",
                        "OwnedShippingAddress.Street",
                        "OwnedShippingAddress.Location.Lat",
                        "OwnedShippingAddress.Location.Lng"
                    ],
                    _singleKeyRowTableInfor,
                    options: updateOptions);

                await connectionContext.BulkUpdateAsync(compositeKeyRows, ["Id1", "Id2"],
                    ["Column3", "Column2"],
                    _compositeKeyRowTableInfor,
                    options: updateOptions);
            }
        }

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        for (var i = 0; i < 100; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].ComplexShippingAddress?.Street, dbRows[i].ComplexShippingAddress?.Street);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lat, dbRows[i].ComplexShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].ComplexShippingAddress?.Location?.Lng, dbRows[i].ComplexShippingAddress?.Location?.Lng);
            Assert.Equal(rows[i].OwnedShippingAddress?.Street, dbRows[i].OwnedShippingAddress?.Street);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lat, dbRows[i].OwnedShippingAddress?.Location?.Lat);
            Assert.Equal(rows[i].OwnedShippingAddress?.Location?.Lng, dbRows[i].OwnedShippingAddress?.Location?.Lng);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
        }
    }
}