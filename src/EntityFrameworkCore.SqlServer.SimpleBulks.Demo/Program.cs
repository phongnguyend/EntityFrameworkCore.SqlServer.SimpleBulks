using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var dbct = new DemoDbContext())
            {
                dbct.Database.Migrate();

                dbct.BulkDelete(dbct.Set<ConfigurationEntry>().AsNoTracking());

                var configurationEntries = new List<ConfigurationEntry>();

                for (int i = 0; i < 1000; i++)
                {
                    configurationEntries.Add(new ConfigurationEntry
                    {
                        Key = $"Key{i}",
                        Value = $"Value{i}",
                        CreatedDateTime = DateTimeOffset.Now,
                    });
                }

                dbct.BulkInsert(configurationEntries);

                foreach (var row in configurationEntries)
                {
                    row.Key += "xx";
                    row.UpdatedDateTime = DateTimeOffset.Now;
                    row.IsSensitive = true;
                    row.Description = row.Id.ToString();
                }

                dbct.BulkUpdate(configurationEntries,
                    x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description });

                configurationEntries.Add(new ConfigurationEntry
                {
                    Key = $"Key{1001}",
                    Value = $"Value{1001}",
                    CreatedDateTime = DateTimeOffset.Now,
                });

                dbct.BulkMerge(configurationEntries,
                    x => x.Id,
                    x => new { x.Key, x.UpdatedDateTime, x.IsSensitive, x.Description },
                    x => new { x.Key, x.Value, x.IsSensitive, x.CreatedDateTime });
            }

            DbContextTest.Run();
            SqlConnectionTest.Run();

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}
