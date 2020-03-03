using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
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
        private const string _connectionString = "Server=.;Database=SimpleBulks;User Id=sa;Password=sqladmin123!@#";

        static void Main(string[] args)
        {
            TableMapper.Register(typeof(Row), "Rows");
            TableMapper.Register(typeof(CompositeKeyRow), "CompositeKeyRows");

            using (var dbct = new DemoDbContext(_connectionString))
            {
                dbct.Database.Migrate();
            }

            //InsertUsingEF(numberOfRows: 500000);
            //UpdateUsingEF();

            DeleteUsingBulkDelete(useLinq: true, omitTableName: true);

            InsertUsingBulkInsert(numberOfRows: 50000, useLinq: true, omitTableName: true);
            UpdateUsingBulkUpdate(useLinq: true, omitTableName: true);
            DeleteUsingBulkDelete(useLinq: true, omitTableName: true);

            InsertUsingBulkInsert(numberOfRows: 50000, useLinq: true, omitTableName: false);
            UpdateUsingBulkUpdate(useLinq: true, omitTableName: false);
            DeleteUsingBulkDelete(useLinq: true, omitTableName: false);

            InsertUsingBulkInsert(numberOfRows: 50000, useLinq: false, omitTableName: true);
            UpdateUsingBulkUpdate(useLinq: false, omitTableName: true);
            DeleteUsingBulkDelete(useLinq: false, omitTableName: true);

            InsertUsingBulkInsert(numberOfRows: 50000, useLinq: false, omitTableName: false);
            UpdateUsingBulkUpdate(useLinq: false, omitTableName: false);
            DeleteUsingBulkDelete(useLinq: false, omitTableName: false);

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

        private static void InsertUsingBulkInsert(int numberOfRows, bool useLinq, bool omitTableName)
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
                    if (omitTableName)
                    {
                        dbct.BulkInsert(rows,
                            row => new { row.Column1, row.Column2, row.Column3 });
                        dbct.BulkInsert(compositeKeyRows,
                            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                    }
                    else
                    {
                        dbct.BulkInsert(rows, "Rows",
                            row => new { row.Column1, row.Column2, row.Column3 });
                        dbct.BulkInsert(compositeKeyRows, "CompositeKeyRows",
                            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                    }

                }
                else
                {
                    if (omitTableName)
                    {
                        dbct.BulkInsert(rows,
                            new string[] { "Column1", "Column2", "Column3" });
                        dbct.BulkInsert(rows.Take(1000),
                            typeof(Row).GetDbColumnNames("Id"));

                        dbct.BulkInsert(compositeKeyRows,
                            new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }
                    else
                    {
                        dbct.BulkInsert(rows, "Rows",
                            new string[] { "Column1", "Column2", "Column3" });
                        dbct.BulkInsert(rows.Take(1000), "Rows",
                            typeof(Row).GetDbColumnNames("Id"));

                        dbct.BulkInsert(compositeKeyRows, "CompositeKeyRows",
                            new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }

                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void UpdateUsingBulkUpdate(bool useLinq, bool omitTableName)
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
                    if (omitTableName)
                    {
                        dbct.BulkUpdate(rows,
                            row => row.Id,
                            row => new { row.Column3, row.Column2 });
                        dbct.BulkUpdate(compositeKeyRows,
                            row => new { row.Id1, row.Id2 },
                            row => new { row.Column3, row.Column2 });
                    }
                    else
                    {
                        dbct.BulkUpdate(rows, "Rows",
                            row => row.Id,
                            row => new { row.Column3, row.Column2 });
                        dbct.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
                            row => new { row.Id1, row.Id2 },
                            row => new { row.Column3, row.Column2 });
                    }

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

                    if (omitTableName)
                    {
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
                        dbct.BulkMerge(rows, "Rows",
                            row => row.Id,
                            row => new { row.Column1, row.Column2 },
                            row => new { row.Column1, row.Column2, row.Column3 });
                        dbct.BulkMerge(compositeKeyRows, "CompositeKeyRows",
                            row => new { row.Id1, row.Id2 },
                            row => new { row.Column1, row.Column2, row.Column3 },
                            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                    }

                }
                else
                {
                    if (omitTableName)
                    {
                        dbct.BulkUpdate(rows,
                            "Id",
                            new string[] { "Column3", "Column2" });
                        dbct.BulkUpdate(compositeKeyRows,
                            new string[] { "Id1", "Id2" },
                            new string[] { "Column3", "Column2" });
                    }
                    else
                    {
                        dbct.BulkUpdate(rows, "Rows",
                            "Id",
                            new string[] { "Column3", "Column2" });
                        dbct.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
                            new string[] { "Id1", "Id2" },
                            new string[] { "Column3", "Column2" });
                    }

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

                    if (omitTableName)
                    {
                        dbct.BulkMerge(rows,
                            "Id",
                            new string[] { "Column1", "Column2" },
                            new string[] { "Column1", "Column2", "Column3" });
                        dbct.BulkMerge(compositeKeyRows,
                            new string[] { "Id1", "Id2" },
                            new string[] { "Column1", "Column2", "Column3" },
                            new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }
                    else
                    {
                        dbct.BulkMerge(rows, "Rows",
                            "Id",
                            new string[] { "Column1", "Column2" },
                            new string[] { "Column1", "Column2", "Column3" });
                        dbct.BulkMerge(compositeKeyRows, "CompositeKeyRows",
                            new string[] { "Id1", "Id2" },
                            new string[] { "Column1", "Column2", "Column3" },
                            new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void DeleteUsingBulkDelete(bool useLinq, bool omitTableName)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext(_connectionString))
            {
                var rows = dbct.Rows.AsNoTracking().ToList();
                var compositeKeyRows = dbct.CompositeKeyRows.AsNoTracking().ToList();

                if (useLinq)
                {
                    if (omitTableName)
                    {
                        dbct.BulkDelete(rows, row => row.Id);
                        dbct.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 });
                    }
                    else
                    {
                        dbct.BulkDelete(rows, "Rows", row => row.Id);
                        dbct.BulkDelete(compositeKeyRows, "CompositeKeyRows", row => new { row.Id1, row.Id2 });
                    }
                }
                else
                {
                    if (omitTableName)
                    {
                        dbct.BulkDelete(rows, "Id");
                        dbct.BulkDelete(compositeKeyRows, new List<string> { "Id1", "Id2" });
                    }
                    else
                    {
                        dbct.BulkDelete(rows, "Rows", "Id");
                        dbct.BulkDelete(compositeKeyRows, "CompositeKeyRows", new List<string> { "Id1", "Id2" });
                    }
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }
    }
}
