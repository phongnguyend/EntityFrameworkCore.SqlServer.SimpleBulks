# EntityFrameworkCore.SqlServer.SimpleBulks
This is a very simple .netcore library that can help to work with large number of records using the SqlBulkCopy class.
 
## EntityFrameworkCore.SqlServer.SimpleBulks supports:
* Bulk insert
* Bulk update
* Bulk delete

## Overview
This project provide 2 extension methods so that you can use it with your EntityFrameworkCore DbContext class. 

## License
Free to copy, modify and use for whatever you want in your applications.  

## Examples

 ```c#
static void Main(string[] args)
{
    using (var dbct = new DemoDbContext())
    {
        dbct.Database.Migrate();
    }

    //InsertUsingEF();
    //UpdateUsingEF();
    InsertUsingBulkInsert(useLinq: true);
    UpdateUsingBulkUpdate(useLinq: true);
    DeleteUsingBulkDelete(useLinq: true);
    InsertUsingBulkInsert(useLinq: false);
    UpdateUsingBulkUpdate(useLinq: false);
    DeleteUsingBulkDelete(useLinq: false);
    Console.WriteLine("Finished!");
    Console.ReadLine();
}

private static void InsertUsingEF()
{
    Stopwatch watch = new Stopwatch();
    watch.Start();
    using (var dbct = new DemoDbContext())
    {
        var rows = new List<Row>();
        for (int i = 0; i < 500000; i++)
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

    var elapsedTime = watch.Elapsed;
    Console.WriteLine(elapsedTime);
}

private static void UpdateUsingEF()
{
    Stopwatch watch = new Stopwatch();
    watch.Start();
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

    var elapsedTime = watch.Elapsed;
    Console.WriteLine(elapsedTime);
}

private static void InsertUsingBulkInsert(bool useLinq)
{
    Stopwatch watch = new Stopwatch();
    watch.Start();
    using (var dbct = new DemoDbContext())
    {
        var rows = new List<Row>();
        var compositeKeyRows = new List<CompositeKeyRow>();
        for (int i = 0; i < 500000; i++)
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

    var elapsedTime = watch.Elapsed;
    Console.WriteLine(elapsedTime);
}

private static void UpdateUsingBulkUpdate(bool useLinq)
{
    Stopwatch watch = new Stopwatch();
    watch.Start();
    using (var dbct = new DemoDbContext())
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
        }
        else
        {
            dbct.BulkUpdate(rows, "Rows", "Id", "Column3", "Column2");
            dbct.BulkUpdate(compositeKeyRows, "CompositeKeyRows", "Id1,Id2".Split(',').ToList(), "Column3", "Column2");
        }
    }
    watch.Stop();

    var elapsedTime = watch.Elapsed;
    Console.WriteLine(elapsedTime);
}

private static void DeleteUsingBulkDelete(bool useLinq)
{
    Stopwatch watch = new Stopwatch();
    watch.Start();
    using (var dbct = new DemoDbContext())
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

    var elapsedTime = watch.Elapsed;
    Console.WriteLine(elapsedTime);
}

 ```
