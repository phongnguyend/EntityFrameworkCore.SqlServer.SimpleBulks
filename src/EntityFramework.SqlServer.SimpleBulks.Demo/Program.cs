using EntityFramework.SqlServer.SimpleBulks.BulkDelete;
using EntityFramework.SqlServer.SimpleBulks.BulkInsert;
using EntityFramework.SqlServer.SimpleBulks.BulkMerge;
using EntityFramework.SqlServer.SimpleBulks.BulkUpdate;
using EntityFramework.SqlServer.SimpleBulks.Demo.Entities;
using EntityFramework.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;

namespace EntityFramework.SqlServer.SimpleBulks.Demo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (var dbct = new DemoDbContext())
            {
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
        }
    }
}
