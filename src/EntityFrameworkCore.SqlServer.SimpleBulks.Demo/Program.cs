using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Demo;
using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;


using (var dbct = new DemoDbContext())
{
    dbct.Database.Migrate();

    var deleteResult = await dbct.BulkDeleteAsync(dbct.Set<ConfigurationEntry>().AsNoTracking(),
          opt =>
          {
              opt.LogTo = Console.WriteLine;
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
        opt =>
        {
            opt.LogTo = Console.WriteLine;
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
        opt =>
        {
            opt.LogTo = Console.WriteLine;
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
        opt =>
        {
            opt.LogTo = Console.WriteLine;
        });

    Console.WriteLine($"Updated: {mergeResult.UpdatedRows} row(s)");
    Console.WriteLine($"Inserted: {mergeResult.InsertedRows} row(s)");
    Console.WriteLine($"Affected: {mergeResult.AffectedRows} row(s)");
}

Console.WriteLine("Finished!");
Console.ReadLine();