using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    internal class DbContextTest
    {
        public static void Run()
        {
            //InsertUsingEF(numberOfRows: 500000);
            //UpdateUsingEF();

            DeleteUsingBulkDelete();

            InsertUsingBulkInsert(numberOfRows: 50000);
            UpdateUsingBulkUpdate(useLinq: true);
            DeleteUsingBulkDelete();

            InsertUsingBulkInsert(numberOfRows: 50000);
            UpdateUsingBulkUpdate(useLinq: false);
            DeleteUsingBulkDelete();
        }

        private static void InsertUsingEF(int numberOfRows)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext())
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

            using (var dbct = new DemoDbContext())
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

        private static void InsertUsingBulkInsert(int numberOfRows)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext())
            {
                var tran = dbct.Database.BeginTransaction();

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

                dbct.BulkInsert(rows,
                    row => new { row.Column1, row.Column2, row.Column3 },
                    row => row.Id);
                dbct.BulkInsert(compositeKeyRows,
                    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

                tran.Commit();
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void UpdateUsingBulkUpdate(bool useLinq)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext())
            {
                var tran = dbct.Database.BeginTransaction();

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
                    dbct.BulkUpdate(rows,
                        row => row.Id,
                        row => new { row.Column3, row.Column2 });
                    dbct.BulkUpdate(compositeKeyRows,
                        row => new { row.Id1, row.Id2 },
                        row => new { row.Column3, row.Column2 });

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

                    dbct.BulkMerge(rows,
                        row => row.Id,
                        row => new { row.Column1, row.Column2 },
                        row => new { row.Column1, row.Column2, row.Column3 });
                    dbct.BulkMerge(compositeKeyRows,
                        row => new { row.Id1, row.Id2 },
                        row => new { row.Column1, row.Column2, row.Column3 },
                        row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                }
                else
                {
                    dbct.BulkUpdate(rows,
                        "Id",
                        new string[] { "Column3", "Column2" });
                    dbct.BulkUpdate(compositeKeyRows,
                        new string[] { "Id1", "Id2" },
                        new string[] { "Column3", "Column2" });

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

                    dbct.BulkMerge(rows,
                        "Id",
                        new string[] { "Column1", "Column2" },
                        new string[] { "Column1", "Column2", "Column3" });
                    dbct.BulkMerge(compositeKeyRows,
                        new string[] { "Id1", "Id2" },
                        new string[] { "Column1", "Column2", "Column3" },
                        new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                }

                tran.Commit();
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void DeleteUsingBulkDelete()
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext())
            {
                var tran = dbct.Database.BeginTransaction();

                var rows = dbct.Rows.AsNoTracking().ToList();
                var compositeKeyRows = dbct.CompositeKeyRows.AsNoTracking().ToList();

                dbct.BulkDelete(rows, row => row.Id);
                dbct.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 });

                tran.Commit();
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }
    }
}
