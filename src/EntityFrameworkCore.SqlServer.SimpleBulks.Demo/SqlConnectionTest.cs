using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    internal class SqlConnectionTest
    {
        private const string _connectionString = "Server=.;Database=SimpleBulks;User Id=sa;Password=sqladmin123!@#";

        public static void Run()
        {
            TableMapper.Register(typeof(Row), "Rows");
            TableMapper.Register(typeof(CompositeKeyRow), "CompositeKeyRows");

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
        }

        private static void InsertUsingBulkInsert(int numberOfRows, bool useLinq, bool omitTableName)
        {
            var watch = Stopwatch.StartNew();

            using (var connection = new SqlConnection(_connectionString))
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
                        connection.BulkInsert(rows,
                            row => new { row.Column1, row.Column2, row.Column3 },
                            row => row.Id);
                        connection.BulkInsert(compositeKeyRows,
                            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                    }
                    else
                    {
                        connection.BulkInsert(rows, "Rows",
                            row => new { row.Column1, row.Column2, row.Column3 },
                            row => row.Id);
                        connection.BulkInsert(compositeKeyRows, "CompositeKeyRows",
                            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                    }

                }
                else
                {
                    if (omitTableName)
                    {
                        connection.BulkInsert(rows,
                            new [] { "Column1", "Column2", "Column3" });
                        connection.BulkInsert(rows.Take(1000),
                            typeof(Row).GetDbColumnNames("Id"));

                        connection.BulkInsert(compositeKeyRows,
                            new [] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }
                    else
                    {
                        connection.BulkInsert(rows, "Rows",
                            new [] { "Column1", "Column2", "Column3" });
                        connection.BulkInsert(rows.Take(1000), "Rows",
                            typeof(Row).GetDbColumnNames("Id"));

                        connection.BulkInsert(compositeKeyRows, "CompositeKeyRows",
                            new [] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }

                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void UpdateUsingBulkUpdate(bool useLinq, bool omitTableName)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext())
            {
                var connection = dbct.GetSqlConnection();

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
                        connection.BulkUpdate(rows,
                            row => row.Id,
                            row => new { row.Column3, row.Column2 });
                        connection.BulkUpdate(compositeKeyRows,
                            row => new { row.Id1, row.Id2 },
                            row => new { row.Column3, row.Column2 });
                    }
                    else
                    {
                        connection.BulkUpdate(rows, "Rows",
                            row => row.Id,
                            row => new { row.Column3, row.Column2 });
                        connection.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
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
                        connection.BulkMerge(rows,
                            row => row.Id,
                            row => new { row.Column1, row.Column2 },
                            row => new { row.Column1, row.Column2, row.Column3 });
                        connection.BulkMerge(compositeKeyRows,
                            row => new { row.Id1, row.Id2 },
                            row => new { row.Column1, row.Column2, row.Column3 },
                            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                    }
                    else
                    {
                        connection.BulkMerge(rows, "Rows",
                            row => row.Id,
                            row => new { row.Column1, row.Column2 },
                            row => new { row.Column1, row.Column2, row.Column3 });
                        connection.BulkMerge(compositeKeyRows, "CompositeKeyRows",
                            row => new { row.Id1, row.Id2 },
                            row => new { row.Column1, row.Column2, row.Column3 },
                            row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                    }

                }
                else
                {
                    if (omitTableName)
                    {
                        connection.BulkUpdate(rows,
                            "Id",
                            new [] { "Column3", "Column2" });
                        connection.BulkUpdate(compositeKeyRows,
                            new [] { "Id1", "Id2" },
                            new [] { "Column3", "Column2" });
                    }
                    else
                    {
                        connection.BulkUpdate(rows, "Rows",
                            "Id",
                            new [] { "Column3", "Column2" });
                        connection.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
                            new [] { "Id1", "Id2" },
                            new [] { "Column3", "Column2" });
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
                        connection.BulkMerge(rows,
                            "Id",
                            new[] { "Column1", "Column2" },
                            new [] { "Column1", "Column2", "Column3" });
                        connection.BulkMerge(compositeKeyRows,
                            new [] { "Id1", "Id2" },
                            new [] { "Column1", "Column2", "Column3" },
                            new [] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }
                    else
                    {
                        connection.BulkMerge(rows, "Rows",
                            "Id",
                            new [] { "Column1", "Column2" },
                            new [] { "Column1", "Column2", "Column3" });
                        connection.BulkMerge(compositeKeyRows, "CompositeKeyRows",
                            new [] { "Id1", "Id2" },
                            new [] { "Column1", "Column2", "Column3" },
                            new [] { "Id1", "Id2", "Column1", "Column2", "Column3" });
                    }
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }

        private static void DeleteUsingBulkDelete(bool useLinq, bool omitTableName)
        {
            var watch = Stopwatch.StartNew();

            using (var dbct = new DemoDbContext())
            {
                var connection = dbct.GetSqlConnection();

                var rows = dbct.Rows.AsNoTracking().ToList();
                var compositeKeyRows = dbct.CompositeKeyRows.AsNoTracking().ToList();

                if (useLinq)
                {
                    if (omitTableName)
                    {
                        connection.BulkDelete(rows, row => row.Id);
                        connection.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 });
                    }
                    else
                    {
                        connection.BulkDelete(rows, "Rows", row => row.Id);
                        connection.BulkDelete(compositeKeyRows, "CompositeKeyRows", row => new { row.Id1, row.Id2 });
                    }
                }
                else
                {
                    if (omitTableName)
                    {
                        connection.BulkDelete(rows, "Id");
                        connection.BulkDelete(compositeKeyRows, new[] { "Id1", "Id2" });
                    }
                    else
                    {
                        connection.BulkDelete(rows, "Rows", "Id");
                        connection.BulkDelete(compositeKeyRows, "CompositeKeyRows", new[] { "Id1", "Id2" });
                    }
                }
            }

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
        }
    }
}
