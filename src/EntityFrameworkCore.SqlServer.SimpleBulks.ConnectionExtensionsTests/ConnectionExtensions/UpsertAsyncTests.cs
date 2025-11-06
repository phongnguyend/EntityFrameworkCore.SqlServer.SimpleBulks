using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using EntityFrameworkCore.SqlServer.SimpleBulks.Upsert;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class UpsertAsyncTests : BaseTest
{
    private string _schema = "";

    public UpsertAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkMerge")
    {
        TableMapper.Register<SingleKeyRow<int>>(new SqlTableInfor(_schema, "SingleKeyRows")
        {
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            }
        });
        TableMapper.Register<CompositeKeyRow<int, int>>(new SqlTableInfor(_schema, "CompositeKeyRows"));
        TableMapper.Register<ConfigurationEntry>(new SqlTableInfor(_schema, "ConfigurationEntry")
        {
            OutputId = new OutputId
            {
                Name = "Id",
                Mode = OutputIdMode.ServerGenerated,
            }
        });
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
                Column3 = DateTime.Now,
                Season = Season.Winter
            });

            compositeKeyRows.Add(new CompositeKeyRow<int, int>
            {
                Id1 = i,
                Id2 = i,
                Column1 = i,
                Column2 = "" + i,
                Column3 = DateTime.Now,
                Season = Season.Winter
            });
        }

        _context.BulkInsert(rows,
          row => new { row.Column1, row.Column2, row.Column3, row.Season });

        _context.BulkInsert(compositeKeyRows,
         row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season });

        tran.Commit();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_Existing_Using_Linq_With_Transaction(int length)
    {
        SeedData(length);

        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var existingRow = rows.First();

        existingRow.Column2 = "abc";
        existingRow.Column3 = DateTime.Now;
        existingRow.Season = Season.Spring;


        var existingCompositeKeyRows = compositeKeyRows.First();

        existingCompositeKeyRows.Column2 = "abc";
        existingCompositeKeyRows.Column3 = DateTime.Now;
        existingCompositeKeyRows.Season = Season.Spring;

        var result1 = await connectionContext.UpsertAsync(existingRow,
        row => row.Id,
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
    row => new { row.Column1, row.Column2, row.Column3, row.Season },
  options: new BulkMergeOptions()
  {
      LogTo = _output.WriteLine,
      ReturnDbGeneratedId = true
  });

        var result2 = await connectionContext.UpsertAsync(existingCompositeKeyRows,
     row => new { row.Id1, row.Id2 },
         row => new { row.Column1, row.Column2, row.Column3, row.Season },
      row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season },
         options: new BulkMergeOptions()
         {
             LogTo = _output.WriteLine
         });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, result1.AffectedRows);
        Assert.Equal(0, result1.InsertedRows);
        Assert.Equal(1, result1.UpdatedRows);

        Assert.Equal(1, result2.AffectedRows);
        Assert.Equal(0, result2.InsertedRows);
        Assert.Equal(1, result2.UpdatedRows);

        Assert.Equal(rows.Count, dbRows.Count);
        Assert.Equal(compositeKeyRows.Count, dbCompositeKeyRows.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_NonExisting_Using_Linq_With_Transaction(int length)
    {
        SeedData(length);

        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var newRow = new SingleKeyRow<int>
        {
            Column1 = length,
            Column2 = "Inserted using Upsert" + length,
            Column3 = DateTime.Now,
            Season = Season.Summer
        };

        var newCompositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = length,
            Id2 = length,
            Column1 = length,
            Column2 = "Inserted using Upsert" + length,
            Column3 = DateTime.Now,
            Season = Season.Summer
        };

        var result1 = await connectionContext.UpsertAsync(newRow,
                    row => row.Id,
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
             row => new { row.Column1, row.Column2, row.Column3, row.Season },
          options: new BulkMergeOptions()
          {
              LogTo = _output.WriteLine,
              ReturnDbGeneratedId = true
          });

        var result2 = await connectionContext.UpsertAsync(newCompositeKeyRow,
    row => new { row.Id1, row.Id2 },
             row => new { row.Column1, row.Column2, row.Column3, row.Season },
         row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3, row.Season },
       options: new BulkMergeOptions()
       {
           LogTo = _output.WriteLine
       });

        tran.Commit();

        rows.Add(newRow);
        compositeKeyRows.Add(newCompositeKeyRow);

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, result1.AffectedRows);
        Assert.Equal(1, result1.InsertedRows);
        Assert.Equal(0, result1.UpdatedRows);

        Assert.Equal(1, result2.AffectedRows);
        Assert.Equal(1, result2.InsertedRows);
        Assert.Equal(0, result2.UpdatedRows);

        Assert.Equal(rows.Count, dbRows.Count);
        Assert.Equal(compositeKeyRows.Count, dbCompositeKeyRows.Count);

        for (int i = 0; i < length + 1; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_Existing_Using_Dynamic_String_With_Transaction(int length)
    {
        SeedData(length);

        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var existingRow = rows.First();

        existingRow.Column2 = "abc";
        existingRow.Column3 = DateTime.Now;
        existingRow.Season = Season.Spring;


        var existingCompositeKeyRows = compositeKeyRows.First();

        existingCompositeKeyRows.Column2 = "abc";
        existingCompositeKeyRows.Column3 = DateTime.Now;
        existingCompositeKeyRows.Season = Season.Spring;

        var result1 = await connectionContext.UpsertAsync(existingRow,
           ["Id"],
   ["Column1", "Column2", "Column3", "Season"],
     ["Column1", "Column2", "Column3", "Season"],
 options: new BulkMergeOptions()
 {
     LogTo = _output.WriteLine,
     ReturnDbGeneratedId = true
 });

        var result2 = await connectionContext.UpsertAsync(existingCompositeKeyRows,
  ["Id1", "Id2"],
            ["Column1", "Column2", "Column3", "Season"],
      ["Id1", "Id2", "Column1", "Column2", "Column3", "Season"],
         options: new BulkMergeOptions()
         {
             LogTo = _output.WriteLine
         });

        tran.Commit();

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, result1.AffectedRows);
        Assert.Equal(0, result1.InsertedRows);
        Assert.Equal(1, result1.UpdatedRows);

        Assert.Equal(1, result2.AffectedRows);
        Assert.Equal(0, result2.InsertedRows);
        Assert.Equal(1, result2.UpdatedRows);

        Assert.Equal(rows.Count, dbRows.Count);
        Assert.Equal(compositeKeyRows.Count, dbCompositeKeyRows.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_NonExisting_Using_Dynamic_String_With_Transaction(int length)
    {
        SeedData(length);

        _connection.Open();

        var tran = _connection.BeginTransaction();

        var connectionContext = new ConnectionContext(_connection, tran);

        var rows = _context.SingleKeyRows.AsNoTracking().ToList();
        var compositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        var newRow = new SingleKeyRow<int>
        {
            Column1 = length,
            Column2 = "Inserted using Upsert" + length,
            Column3 = DateTime.Now,
            Season = Season.Summer
        };

        var newCompositeKeyRow = new CompositeKeyRow<int, int>
        {
            Id1 = length,
            Id2 = length,
            Column1 = length,
            Column2 = "Inserted using Upsert" + length,
            Column3 = DateTime.Now,
            Season = Season.Summer
        };

        var result1 = await connectionContext.UpsertAsync(newRow,
      ["Id"],
 ["Column1", "Column2", "Column3", "Season"],
    ["Column1", "Column2", "Column3", "Season"],
        options: new BulkMergeOptions()
        {
            LogTo = _output.WriteLine,
            ReturnDbGeneratedId = true
        });

        var result2 = await connectionContext.UpsertAsync(newCompositeKeyRow,
    ["Id1", "Id2"],
  ["Column1", "Column2", "Column3", "Season"],
      ["Id1", "Id2", "Column1", "Column2", "Column3", "Season"],
 options: new BulkMergeOptions()
 {
     LogTo = _output.WriteLine
 });

        tran.Commit();

        rows.Add(newRow);
        compositeKeyRows.Add(newCompositeKeyRow);

        // Assert
        var dbRows = _context.SingleKeyRows.AsNoTracking().ToList();
        var dbCompositeKeyRows = _context.CompositeKeyRows.AsNoTracking().ToList();

        Assert.Equal(1, result1.AffectedRows);
        Assert.Equal(1, result1.InsertedRows);
        Assert.Equal(0, result1.UpdatedRows);

        Assert.Equal(1, result2.AffectedRows);
        Assert.Equal(1, result2.InsertedRows);
        Assert.Equal(0, result2.UpdatedRows);

        Assert.Equal(rows.Count, dbRows.Count);
        Assert.Equal(compositeKeyRows.Count, dbCompositeKeyRows.Count);

        for (int i = 0; i < length + 1; i++)
        {
            Assert.Equal(rows[i].Id, dbRows[i].Id);
            Assert.Equal(rows[i].Column1, dbRows[i].Column1);
            Assert.Equal(rows[i].Column2, dbRows[i].Column2);
            Assert.Equal(rows[i].Column3, dbRows[i].Column3);
            Assert.Equal(rows[i].Season, dbRows[i].Season);

            Assert.Equal(compositeKeyRows[i].Id1, dbCompositeKeyRows[i].Id1);
            Assert.Equal(compositeKeyRows[i].Id2, dbCompositeKeyRows[i].Id2);
            Assert.Equal(compositeKeyRows[i].Column1, dbCompositeKeyRows[i].Column1);
            Assert.Equal(compositeKeyRows[i].Column2, dbCompositeKeyRows[i].Column2);
            Assert.Equal(compositeKeyRows[i].Column3, dbCompositeKeyRows[i].Column3);
            Assert.Equal(compositeKeyRows[i].Season, dbCompositeKeyRows[i].Season);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_NonExisting_ReturnDbGeneratedId_True(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
         new BulkInsertOptions()
         {
             LogTo = _output.WriteLine
         });


        for (int i = length; i < length * 2; i++)
        {
            var row = new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            };

            configurationEntries.Add(row);

            var result = await connectionContext.UpsertAsync(row,
   x => x.Id,
    x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
     x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
             options: new BulkMergeOptions()
             {
                 LogTo = _output.WriteLine
             });

            Assert.Equal(1, result.AffectedRows);
            Assert.Equal(1, result.InsertedRows);
            Assert.Equal(0, result.UpdatedRows);
        }

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(configurationEntries.Count, configurationEntriesInDb.Count);

        for (int i = 0; i < length * 2; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_NonExisting_ReturnDbGeneratedId_False(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
            new BulkInsertOptions()
            {
                LogTo = _output.WriteLine
            });

        for (int i = length; i < length * 2; i++)
        {
            var row = new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            };

            configurationEntries.Add(row);

            var result = await connectionContext.UpsertAsync(row,
                x => x.Id,
          x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
          x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
        options: new BulkMergeOptions()
        {
            ReturnDbGeneratedId = false,
            LogTo = _output.WriteLine
        });

            Assert.Equal(1, result.AffectedRows);
            Assert.Equal(1, result.InsertedRows);
            Assert.Equal(0, result.UpdatedRows);
        }

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(configurationEntries.Count, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }

        for (int i = length; i < length * 2; i++)
        {
            Assert.Equal(Guid.Empty, configurationEntries[i].Id);
            Assert.NotEqual(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_Existing_Should_Update_If_Updated_Columns_Sepecified(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
       new BulkInsertOptions()
       {
           LogTo = _output.WriteLine
       });

        var entry = configurationEntries.First();
        entry.Description = "Updated";
        entry.UpdatedDateTime = DateTimeOffset.Now;

        var result = await connectionContext.UpsertAsync(entry,
    x => x.Id,
     x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
             x => new { },
             options: new BulkMergeOptions()
             {
                 LogTo = _output.WriteLine
             });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(1, result.AffectedRows);
        Assert.Equal(0, result.InsertedRows);
        Assert.Equal(1, result.UpdatedRows);
        Assert.Equal(length, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_Existing_Should_Update_If_Both_Updated_Inserted_Columns_Sepecified(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
new BulkInsertOptions()
{
    LogTo = _output.WriteLine
});

        var entry = configurationEntries.First();
        entry.Description = "Updated";
        entry.UpdatedDateTime = DateTimeOffset.Now;

        var result = await connectionContext.UpsertAsync(entry,
                x => x.Id,
       x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
        x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
          options: new BulkMergeOptions()
          {
              LogTo = _output.WriteLine
          });

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(1, result.AffectedRows);
        Assert.Equal(0, result.InsertedRows);
        Assert.Equal(1, result.UpdatedRows);
        Assert.Equal(length, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_NonExisting_Should_Insert_If_Inserted_Columns_Sepecified(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
            new BulkInsertOptions()
            {
                LogTo = _output.WriteLine
            });

        for (int i = length; i < length * 2; i++)
        {
            var row = new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            };

            configurationEntries.Add(row);

            var result = await connectionContext.UpsertAsync(row,
       x => x.Id,
            x => new { },
    x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
                options: new BulkMergeOptions()
                {
                    LogTo = _output.WriteLine
                });

            Assert.Equal(1, result.AffectedRows);
            Assert.Equal(1, result.InsertedRows);
            Assert.Equal(0, result.UpdatedRows);
        }

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length * 2, configurationEntriesInDb.Count);

        for (int i = 0; i < length * 2; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_NonExisting_Should_Insert_If_Both_Updated_Inserted_Columns_Sepecified(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
                   new BulkInsertOptions()
                   {
                       LogTo = _output.WriteLine
                   });

        for (int i = length; i < length * 2; i++)
        {
            var row = new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            };

            configurationEntries.Add(row);

            var result = await connectionContext.UpsertAsync(row,
                     x => x.Id,
              x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
                     x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
          options: new BulkMergeOptions()
          {
              LogTo = _output.WriteLine
          });

            Assert.Equal(1, result.AffectedRows);
            Assert.Equal(1, result.InsertedRows);
            Assert.Equal(0, result.UpdatedRows);
        }

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();

        Assert.Equal(length * 2, configurationEntriesInDb.Count);

        for (int i = 0; i < length * 2; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_Existing_Do_Nothing(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
       new BulkInsertOptions()
       {
           LogTo = _output.WriteLine
       });

        var entry = configurationEntries.First();
        entry.Description = "Updated";
        entry.UpdatedDateTime = DateTimeOffset.Now;

        var result1 = await connectionContext.UpsertAsync(entry,
                x => x.Id,
                x => new { },
                x => new { },
                options: new BulkMergeOptions()
                {
                    LogTo = _output.WriteLine
                });

        Assert.Equal(0, result1.AffectedRows);
        Assert.Equal(0, result1.InsertedRows);
        Assert.Equal(0, result1.UpdatedRows);

        var result2 = await connectionContext.UpsertAsync(entry,
 x => x.Id,
      x => new { },
            x => new { x.Key, x.Value, x.Description, x.IsSensitive, x.CreatedDateTime },
  options: new BulkMergeOptions()
  {
      LogTo = _output.WriteLine
  });

        Assert.Equal(0, result2.AffectedRows);
        Assert.Equal(0, result2.InsertedRows);
        Assert.Equal(0, result2.UpdatedRows);

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();


        Assert.Equal(length, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            if (configurationEntries[i].Id == entry.Id)
            {
                Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
                Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
                Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
                Assert.NotEqual(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
                Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
                Assert.NotEqual(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
            }
            else
            {
                Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
                Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
                Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
                Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
                Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
                Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    public async Task Upsert_NonExisting_Do_Nothing(int length)
    {
        var connectionContext = new ConnectionContext(_connection, null);

        var configurationEntries = new List<ConfigurationEntry>();

        for (int i = 0; i < length; i++)
        {
            configurationEntries.Add(new ConfigurationEntry
            {
                Key = $"Key{i}",
                Value = $"Value{i}",
                Description = string.Empty,
                CreatedDateTime = DateTimeOffset.Now,
            });
        }

        _context.BulkInsert(configurationEntries,
       new BulkInsertOptions()
       {
           LogTo = _output.WriteLine
       });

        var entry = new ConfigurationEntry
        {
            Key = $"Key{length}",
            Value = $"Value{length}",
            Description = string.Empty,
            CreatedDateTime = DateTimeOffset.Now,
        };

        var result1 = await connectionContext.UpsertAsync(entry,
            x => x.Id,
    x => new { },
     x => new { },
        options: new BulkMergeOptions()
        {
            LogTo = _output.WriteLine
        });

        Assert.Equal(0, result1.AffectedRows);
        Assert.Equal(0, result1.InsertedRows);
        Assert.Equal(0, result1.UpdatedRows);

        var result2 = await connectionContext.UpsertAsync(entry,
     x => x.Id,
       x => new { x.Key, x.Value, x.Description, x.UpdatedDateTime },
        x => new { },
    options: new BulkMergeOptions()
    {
        LogTo = _output.WriteLine
    });

        Assert.Equal(0, result2.AffectedRows);
        Assert.Equal(0, result2.InsertedRows);
        Assert.Equal(0, result2.UpdatedRows);

        // Assert
        var configurationEntriesInDb = _context.Set<ConfigurationEntry>().AsNoTracking().ToList();


        Assert.Equal(length, configurationEntriesInDb.Count);

        for (int i = 0; i < length; i++)
        {
            Assert.Equal(configurationEntries[i].Id, configurationEntriesInDb[i].Id);
            Assert.Equal(configurationEntries[i].Key, configurationEntriesInDb[i].Key);
            Assert.Equal(configurationEntries[i].Value, configurationEntriesInDb[i].Value);
            Assert.Equal(configurationEntries[i].Description, configurationEntriesInDb[i].Description);
            Assert.Equal(configurationEntries[i].CreatedDateTime, configurationEntriesInDb[i].CreatedDateTime);
            Assert.Equal(configurationEntries[i].UpdatedDateTime, configurationEntriesInDb[i].UpdatedDateTime);
        }
    }
}