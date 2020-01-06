using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    class Program
    {
        private const string _connectionString = "Server=.;Database=SimpleBulks;User Id=sa;Password=sqladmin123!@#;MultipleActiveResultSets=true";

        static void Main(string[] args)
        {
            using (var dbct = new DemoDbContext(_connectionString))
            {
                dbct.Database.Migrate();
            }

            //InsertUsingEF(numberOfRows: 500000);
            //UpdateUsingEF();
            InsertUsingBulkInsert(numberOfRows: 500000, useLinq: true);
            UpdateUsingBulkUpdate(useLinq: true);
            DeleteUsingBulkDelete(useLinq: true);
            InsertUsingBulkInsert(numberOfRows: 500000, useLinq: false);
            UpdateUsingBulkUpdate(useLinq: false);
            DeleteUsingBulkDelete(useLinq: false);
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        private static void InsertUsingEF(int numberOfRows)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext(_connectionString))
            {
                var rows = new List<Row>();
                for (int i = 0; i < numberOfRows; i++)
                {
                    rows.Add(new Row
                    {
                        Column1 = i,
                        Column2 = "" + i,
                        Column3 = DateTime.Now
                    });
                }

                dbct.Rows.AddRange(rows);
                dbct.SaveChanges();
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void UpdateUsingEF()
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext(_connectionString))
            {
                var rows = dbct.Rows.ToList();

                foreach (var row in rows)
                {
                    row.Column2 = "abc";
                    row.Column3 = DateTime.Now;
                }

                dbct.SaveChanges();
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void InsertUsingBulkInsert(int numberOfRows, bool useLinq)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext(_connectionString))
            {
                var rows = new List<Row>();
                var compositeKeyRows = new List<CompositeKeyRow>();
                for (int i = 0; i < numberOfRows; i++)
                {
                    rows.Add(new Row
                    {
                        Column1 = i,
                        Column2 = "" + i,
                        Column3 = DateTime.Now
                    });

                    compositeKeyRows.Add(new CompositeKeyRow
                    {
                        Id1 = i,
                        Id2 = i,
                        Column1 = i,
                        Column2 = "" + i,
                        Column3 = DateTime.Now
                    });
                }

                if (useLinq)
                {
                    dbct.BulkInsert(rows, "Rows", row => new { row.Column1, row.Column2, row.Column3 });
                    dbct.BulkInsert(compositeKeyRows, "CompositeKeyRows", row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                }
                else
                {
                    dbct.BulkInsert(rows, "Rows", "Column1", "Column2", "Column3");
                    dbct.BulkInsert(compositeKeyRows, "CompositeKeyRows", "Id1", "Id2", "Column1", "Column2", "Column3");
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void UpdateUsingBulkUpdate(bool useLinq)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext(_connectionString))
            {
                var rows = dbct.Rows.AsNoTracking().ToList();
                var compositeKeyRows = dbct.CompositeKeyRows.AsNoTracking().ToList();

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
                    dbct.BulkUpdate(rows, "Rows", row => row.Id, row => new { row.Column3, row.Column2 });
                    dbct.BulkUpdate(compositeKeyRows, "CompositeKeyRows", row => new { row.Id1, row.Id2 }, row => new { row.Column3, row.Column2 });

                    var newId = rows.Max(x => x.Id) + 1;

                    rows.Add(new Row
                    {
                        Id = newId,
                        Column1 = newId,
                        Column2 = "Inserted using Merge" + newId,
                        Column3 = DateTime.Now,
                    });

                    var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
                    var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

                    compositeKeyRows.Add(new CompositeKeyRow
                    {
                        Id1 = newId1,
                        Id2 = newId2,
                        Column1 = newId2,
                        Column2 = "Inserted using Merge" + newId2,
                        Column3 = DateTime.Now,
                    });

                    dbct.BulkMerge(rows, "Rows",
                        row => row.Id,
                        row => new { row.Column1, row.Column2 },
                        row => new { row.Column1, row.Column2, row.Column3 });
                    dbct.BulkMerge(compositeKeyRows, "CompositeKeyRows",
                        row => new { row.Id1, row.Id2 },
                        row => new { row.Column1, row.Column2, row.Column3 },
                        row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                }
                else
                {
                    dbct.BulkUpdate(rows, "Rows", "Id", "Column3", "Column2");
                    dbct.BulkUpdate(compositeKeyRows, "CompositeKeyRows", "Id1,Id2".Split(',').ToList(), "Column3", "Column2");

                    var newId = rows.Max(x => x.Id) + 1;

                    rows.Add(new Row
                    {
                        Id = newId,
                        Column1 = newId,
                        Column2 = "Inserted using Merge" + newId,
                        Column3 = DateTime.Now,
                    });

                    var newId1 = compositeKeyRows.Max(x => x.Id1) + 1;
                    var newId2 = compositeKeyRows.Max(x => x.Id2) + 1;

                    compositeKeyRows.Add(new CompositeKeyRow
                    {
                        Id1 = newId1,
                        Id2 = newId2,
                        Column1 = newId2,
                        Column2 = "Inserted using Merge" + newId2,
                        Column3 = DateTime.Now,
                    });

                    dbct.BulkMerge(rows, "Rows",
                        "Id",
                        new string[] { "Column1", "Column2" },
                        new string[] { "Column1", "Column2", "Column3" });
                    dbct.BulkMerge(compositeKeyRows, "CompositeKeyRows",
                        new List<string> { "Id1", "Id2" },
                        new string[] { "Column1", "Column2", "Column3" },
                        new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void DeleteUsingBulkDelete(bool useLinq)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext(_connectionString))
            {
                var rows = dbct.Rows.AsNoTracking().ToList();
                var compositeKeyRows = dbct.CompositeKeyRows.AsNoTracking().ToList();

                if (useLinq)
                {
                    dbct.BulkDelete(rows, "Rows", row => row.Id);
                    dbct.BulkDelete(compositeKeyRows, "CompositeKeyRows", row => new { row.Id1, row.Id2 });
                }
                else
                {
                    dbct.BulkDelete(rows, "Rows", "Id");
                    dbct.BulkDelete(compositeKeyRows, "CompositeKeyRows", "Id1,Id2".Split(',').ToList());
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }
    }
}
