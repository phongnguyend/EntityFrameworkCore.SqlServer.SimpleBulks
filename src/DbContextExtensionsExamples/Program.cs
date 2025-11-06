using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using DbContextExtensionsExamples;
using DbContextExtensionsExamples.Entities;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using (var dbct = new DemoDbContext())
{
    dbct.Database.Migrate();

    var deleteResult = await dbct.BulkDeleteAsync(dbct.Set<ConfigurationEntry>().AsNoTracking(),
        new BulkDeleteOptions()
        {
            LogTo = Console.WriteLine
        });

    Console.WriteLine($"Deleted: {deleteResult.AffectedRows} row(s)");

    var configurationEntries = new List<ConfigurationEntry>();

    for (int i = 0; i < 1000; i++)
    {
        configurationEntries.Add(new ConfigurationEntry
        {
            Key = $"Key{i}",
            Value = $"Value{i}",
            CreatedDateTime = DateTimeOffset.Now,
            SeasonAsInt = Season.Spring,
            SeasonAsString = Season.Spring,
        });
    }

    await dbct.BulkInsertAsync(configurationEntries,
        new BulkInsertOptions()
        {
            LogTo = Console.WriteLine
        });

    foreach (var row in configurationEntries)
    {
        row.Key += "xx";
        row.UpdatedDateTime = DateTimeOffset.Now;
        row.SeasonAsInt = Season.Winter;
        row.SeasonAsString = Season.Winter;
        row.IsSensitive = true;
        row.Description = row.Id.ToString();
    }

    var updateResult = await dbct.BulkUpdateAsync(configurationEntries,
        x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description, x.SeasonAsInt, x.SeasonAsString },
        new BulkUpdateOptions()
        {
            LogTo = Console.WriteLine
        });

    Console.WriteLine($"Updated: {updateResult.AffectedRows} row(s)");

    configurationEntries.Add(new ConfigurationEntry
    {
        Key = $"Key{1001}",
        Value = $"Value{1001}",
        CreatedDateTime = DateTimeOffset.Now,
        SeasonAsInt = Season.Summer,
        SeasonAsString = Season.Summer,
    });

    var mergeResult = await dbct.BulkMergeAsync(configurationEntries,
        x => x.Id,
        x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description },
        x => new { x.Key, x.Value, x.IsSensitive, x.CreatedDateTime, x.SeasonAsInt, x.SeasonAsString },
        new BulkMergeOptions()
        {
            LogTo = Console.WriteLine
        });

    Console.WriteLine($"Updated: {mergeResult.UpdatedRows} row(s)");
    Console.WriteLine($"Inserted: {mergeResult.InsertedRows} row(s)");
    Console.WriteLine($"Affected: {mergeResult.AffectedRows} row(s)");

    var configurationEntry = new ConfigurationEntry
    {
        Key = $"Key_DirectInsert",
        Value = $"Value_DirectInsert",
        CreatedDateTime = DateTimeOffset.Now,
        SeasonAsInt = Season.Autumn,
        SeasonAsString = Season.Autumn,
    };

    await dbct.DirectInsertAsync(configurationEntry,
        new BulkInsertOptions()
        {
            LogTo = Console.WriteLine
        });

    configurationEntry.Key += "_Updated";
    configurationEntry.Value += "_Updated";
    configurationEntry.UpdatedDateTime = DateTimeOffset.Now;
    configurationEntry.SeasonAsInt = Season.Spring;
    configurationEntry.SeasonAsString = Season.Spring;

    await dbct.DirectUpdateAsync(configurationEntry,
        x => new { x.Key, x.Value, x.UpdatedDateTime, x.SeasonAsInt, x.SeasonAsString },
        new BulkUpdateOptions()
        {
            LogTo = Console.WriteLine
        });

    await dbct.DirectDeleteAsync(configurationEntry,
        new BulkDeleteOptions()
        {
            LogTo = Console.WriteLine
        });
}

Console.WriteLine("Finished!");
Console.ReadLine();