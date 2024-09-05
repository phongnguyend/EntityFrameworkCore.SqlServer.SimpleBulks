# EntityFrameworkCore.SqlServer.SimpleBulks
A very simple .net core library that can help to sync a large number of records in-memory into the database using the **SqlBulkCopy** class.
 
## Overview
This library provides extension methods so that you can use with your EntityFrameworkCore **DbContext** instance:
**DbContextExtensions.cs**
or you can use **SqlConnectionExtensions.cs** to work directly with a **SqlConnection** instance.

## Nuget
https://www.nuget.org/packages/EntityFrameworkCore.SqlServer.SimpleBulks

## EntityFrameworkCore.SqlServer.SimpleBulks supports:
- Bulk Insert
- Bulk Update
- Bulk Delete
- Bulk Merge

## Examples
- Refer [EntityFrameworkCore.SqlServer.SimpleBulks.Demo](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Demo/Program.cs)

- Update the connection string:
  ```c#
  private const string _connectionString = "Server=.;Database=SimpleBulks;User Id=xxx;Password=xxx";
  ```
- Build and run.

## DbContextExtensions:
### Using Lambda Expression:
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

// Insert all columns
dbct.BulkInsert(rows);
dbct.BulkInsert(compositeKeyRows);

// Insert selected columns only
dbct.BulkInsert(rows,
    row => new { row.Column1, row.Column2, row.Column3 });
dbct.BulkInsert(compositeKeyRows,
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

dbct.BulkUpdate(rows,
    row => new { row.Column3, row.Column2 });
dbct.BulkUpdate(compositeKeyRows,
    row => new { row.Column3, row.Column2 });

dbct.BulkMerge(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    options =>
    {
        //options.WithHoldLock = true;
    });
dbct.BulkMerge(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
    options =>
    {
        //options.WithHoldLock = true;
    });
                        
dbct.BulkDelete(rows);
dbct.BulkDelete(compositeKeyRows);
```
### Using Dynamic String:
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

dbct.BulkUpdate(rows,
    new [] { "Column3", "Column2" });
dbct.BulkUpdate(compositeKeyRows,
    new [] { "Column3", "Column2" });

dbct.BulkMerge(rows,
    "Id",
    new [] { "Column1", "Column2" },
    new [] { "Column1", "Column2", "Column3" },
    options =>
    {
        //options.WithHoldLock = true;
    });
dbct.BulkMerge(compositeKeyRows,
    new [] { "Id1", "Id2" },
    new [] { "Column1", "Column2", "Column3" },
    new [] { "Id1", "Id2", "Column1", "Column2", "Column3" },
    options =>
    {
        //options.WithHoldLock = true;
    });
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression:
```c#
new BulkInsertBuilder<Row>(dbct.GetSqlConnection())
	.WithData(rows)
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns(new [] { "Column1", "Column2", "Column3" })
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable(dbct.GetTableName(typeof(Row)))
	// or .ToTable("Rows")
	.Execute();
```

## SqlConnectionExtensions:
### Using Lambda Expression:
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

// Register Type - Table Name globaly
TableMapper.Register(typeof(Row), "Rows");
TableMapper.Register(typeof(CompositeKeyRow), "CompositeKeyRows");

connection.BulkInsert(rows,
           row => new { row.Column1, row.Column2, row.Column3 });
connection.BulkInsert(compositeKeyRows,
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

connection.BulkUpdate(rows,
           row => row.Id,
           row => new { row.Column3, row.Column2 });
connection.BulkUpdate(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column3, row.Column2 });

connection.BulkMerge(rows,
           row => row.Id,
           row => new { row.Column1, row.Column2 },
           row => new { row.Column1, row.Column2, row.Column3 },
           options =>
           {
               //options.WithHoldLock = true;
           });
connection.BulkMerge(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column1, row.Column2, row.Column3 },
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 },
           options =>
           {
               //options.WithHoldLock = true;
           });
                        
connection.BulkDelete(rows, row => row.Id);
connection.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 });
```
### Using Dynamic String:
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

connection.BulkInsert(rows, "Rows",
           new [] { "Column1", "Column2", "Column3" });
connection.BulkInsert(rows.Take(1000), "Rows",
           typeof(Row).GetDbColumnNames("Id"));
connection.BulkInsert(compositeKeyRows, "CompositeKeyRows",
           new [] { "Id1", "Id2", "Column1", "Column2", "Column3" });

connection.BulkUpdate(rows, "Rows",
           "Id",
           new [] { "Column3", "Column2" });
connection.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
           new [] { "Id1", "Id2" },
           new [] { "Column3", "Column2" });

connection.BulkMerge(rows, "Rows",
           "Id",
           new [] { "Column1", "Column2" },
           new [] { "Column1", "Column2", "Column3" },
           options =>
           {
               //options.WithHoldLock = true;
           });
connection.BulkMerge(compositeKeyRows, "CompositeKeyRows",
           new [] { "Id1", "Id2" },
           new [] { "Column1", "Column2", "Column3" },
           new [] { "Id1", "Id2", "Column1", "Column2", "Column3" },
           options =>
           {
               //options.WithHoldLock = true;
           });

connection.BulkDelete(rows, "Rows", "Id");
connection.BulkDelete(compositeKeyRows, "CompositeKeyRows", new [] { "Id1", "Id2" });
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression:
```c#
new BulkInsertBuilder<Row>(connection)
	.WithData(rows)
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns(new [] { "Column1", "Column2", "Column3" })
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable("Rows")
	.Execute();
```

## Configuration
### BulkInsert
```c#
_context.BulkInsert(rows,
		row => new { row.Column1, row.Column2, row.Column3 },
		options =>
		{
			options.KeepIdentity = false;
			options.BatchSize = 0;
			options.Timeout = 30;
			options.LogTo = Console.WriteLine;
		});
```
### BulkUpdate
```c#
_context.BulkUpdate(rows,
		row => new { row.Column3, row.Column2 },
		options =>
		{
			options.BatchSize = 0;
			options.Timeout = 30;
			options.LogTo = Console.WriteLine;
		});
```
### BulkMerge
```c#
_context.BulkMerge(rows,
		row => row.Id,
		row => new { row.Column1, row.Column2 },
		row => new { row.Column1, row.Column2, row.Column3 },
		options =>
		{
			options.BatchSize = 0;
			options.Timeout = 30;
			options.WithHoldLock = false;
			options.LogTo = Console.WriteLine;
		});
```
### BulkDelete
```c#
_context.BulkDelete(rows,
		options =>
		{
			options.BatchSize = 0;
			options.Timeout = 30;
			options.LogTo = Console.WriteLine;
		});
```

## Benchmarks
### BulkInsert
Single Table [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs)

![alt text](/docs/benchmarks/bulkinsert/imgs/bulk-insert-single-table.png)

Multiple Tables (1x parent rows + 5x child rows) [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs)

![alt text](/docs/benchmarks/bulkinsert/imgs/bulk-insert-multiple-tables.png)

### BulkUpdate
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs)

![alt text](/docs/benchmarks/bulkupdate/imgs/bulk-update.png)

### BulkDelete
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs)

![alt text](/docs/benchmarks/bulkdelete/imgs/bulk-delete.png)

### BulkMerge
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs)

![alt text](/docs/benchmarks/bulkmerge/imgs/bulk-merge.png)

### BulkMatch
Single Column [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs)

![alt text](/docs/benchmarks/bulkmatch/imgs/bulk-match-single-column.png)

## License
**EntityFrameworkCore.SqlServer.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
