# EntityFrameworkCore.SqlServer.SimpleBulks
A very simple .net core library that can help to sync a large number of records in-memory into the database using the **SqlBulkCopy** class.
 
## Overview
This library provides extension methods so that you can use with your EntityFrameworkCore **DbContext** instance **DbContextExtensions.cs**
or you can use **SqlConnectionExtensions.cs** to work directly with a **SqlConnection** instance without using EntityFrameworkCore.

## Nuget
https://www.nuget.org/packages/EntityFrameworkCore.SqlServer.SimpleBulks

## Features
- Bulk Insert
- Bulk Update
- Bulk Delete
- Bulk Merge
- Bulk Match
- Temp Table
- Direct Insert
- Direct Update
- Direct Delete

## Examples
[EntityFrameworkCore.SqlServer.SimpleBulks.Demo](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Demo/Program.cs)
- Update the connection string:
  ```c#
  private const string _connectionString = "Server=.;Database=SimpleBulks;User Id=xxx;Password=xxx";
  ```
- Build and run.

## DbContextExtensions
### Using Lambda Expression
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
    row => new { row.Column1, row.Column2, row.Column3 });
dbct.BulkMerge(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
dbct.BulkDelete(rows);
dbct.BulkDelete(compositeKeyRows);
```
### Using Dynamic String
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

dbct.BulkUpdate(rows,
    [ "Column3", "Column2" ]);
dbct.BulkUpdate(compositeKeyRows,
    [ "Column3", "Column2" ]);

dbct.BulkMerge(rows,
    "Id",
    [ "Column1", "Column2" ],
    [ "Column1", "Column2", "Column3" ]);
dbct.BulkMerge(compositeKeyRows,
    [ "Id1", "Id2" ],
    [ "Column1", "Column2", "Column3" ],
    [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression
```c#
new BulkInsertBuilder<Row>(dbct.GetSqlConnection())
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable(dbct.GetTableName(typeof(Row)))
	// or .ToTable("Rows")
	.Execute(rows);
```

## SqlConnectionExtensions
### Using Lambda Expression
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
           row => new { row.Column1, row.Column2, row.Column3 });
connection.BulkMerge(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column1, row.Column2, row.Column3 },
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
connection.BulkDelete(rows, row => row.Id);
connection.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 });
```
### Using Dynamic String
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

connection.BulkInsert(rows, "Rows",
           [ "Column1", "Column2", "Column3" ]);
connection.BulkInsert(rows.Take(1000), "Rows",
           typeof(Row).GetDbColumnNames("Id"));
connection.BulkInsert(compositeKeyRows, "CompositeKeyRows",
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

connection.BulkUpdate(rows, "Rows",
           "Id",
           [ "Column3", "Column2" ]);
connection.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
           [ "Id1", "Id2" ],
           [ "Column3", "Column2" ]);

connection.BulkMerge(rows, "Rows",
           "Id",
           [ "Column1", "Column2" ],
           [ "Column1", "Column2", "Column3" ]);
connection.BulkMerge(compositeKeyRows, "CompositeKeyRows",
           [ "Id1", "Id2" ],
           [ "Column1", "Column2", "Column3" ],
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

connection.BulkDelete(rows, "Rows", "Id");
connection.BulkDelete(compositeKeyRows, "CompositeKeyRows", [ "Id1", "Id2" ]);
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression
```c#
new BulkInsertBuilder<Row>(connection)
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable("Rows")
	.Execute(rows);
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
        options.ReturnDbGeneratedId = true;
        options.LogTo = Console.WriteLine;
    });
```
### BulkMatch
```c#
var contactsFromDb = _context.BulkMatch(matchedContacts,
    x => new { x.CustomerId, x.CountryIsoCode },
    options =>
    {
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### TempTable
```c#
var customerTableName = _context.CreateTempTable(customers,
    x => new
    {
        x.IdNumber,
        x.FirstName,
        x.LastName,
        x.CurrentCountryIsoCode
    },
    options =>
    {
        options.BatchSize = 0;
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### DirectInsert
```c#
_context.DirectInsert(row,
    row => new { row.Column1, row.Column2, row.Column3 },
    options =>
    {
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### DirectUpdate
```c#
_context.DirectUpdate(row,
    row => new { row.Column3, row.Column2 },
    options =>
    {
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```
### DirectDelete
```c#
_context.DirectDelete(row,
    options =>
    {
        options.Timeout = 30;
        options.LogTo = Console.WriteLine;
    });
```

## Returned Result
### BulkUpdate
```c#
var updateResult = dbct.BulkUpdate(rows, row => new { row.Column3, row.Column2 });

Console.WriteLine($"Updated: {updateResult.AffectedRows} row(s)");
```
### BulkDelete
```c#
var deleteResult = dbct.BulkDelete(rows);

Console.WriteLine($"Deleted: {deleteResult.AffectedRows} row(s)");
```
### BulkMerge
```c#
var mergeResult = dbct.BulkMerge(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 });

Console.WriteLine($"Updated: {mergeResult.UpdatedRows} row(s)");
Console.WriteLine($"Inserted: {mergeResult.InsertedRows} row(s)");
Console.WriteLine($"Affected: {mergeResult.AffectedRows} row(s)");
```

## Benchmarks
### BulkInsert
Single Table [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertSingleTableBenchmarks.cs)

![alt text](/docs/benchmarks/bulkinsert/imgs/bulk-insert-single-table.png)

Multiple Tables (1x parent rows + 5x child rows) [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs)

![alt text](/docs/benchmarks/bulkinsert/imgs/bulk-insert-multiple-tables.png)

### BulkUpdate
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs)

![alt text](/docs/benchmarks/bulkupdate/imgs/bulk-update-1.png)

![alt text](/docs/benchmarks/bulkupdate/imgs/bulk-update-2.png)

### BulkDelete
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs)

![alt text](/docs/benchmarks/bulkdelete/imgs/bulk-delete-1.png)

![alt text](/docs/benchmarks/bulkdelete/imgs/bulk-delete-2.png)

### BulkMerge
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs)

![alt text](/docs/benchmarks/bulkmerge/imgs/bulk-merge.png)

[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeReturnDbGeneratedIdBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeReturnDbGeneratedIdBenchmarks.cs)

![alt text](/docs/benchmarks/bulkmerge/imgs/bulk-merge-db-generated-id-1.png)

![alt text](/docs/benchmarks/bulkmerge/imgs/bulk-merge-db-generated-id-2.png)

### BulkMatch
Single Column [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs)

![alt text](/docs/benchmarks/bulkmatch/imgs/bulk-match-single-column-1.png)

![alt text](/docs/benchmarks/bulkmatch/imgs/bulk-match-single-column-2.png)

Multiple Columns [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs)

![alt text](/docs/benchmarks/bulkmatch/imgs/bulk-match-multiple-columns-1.png)

![alt text](/docs/benchmarks/bulkmatch/imgs/bulk-match-multiple-columns-2.png)

### TempTable
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/TempTableBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/TempTableBenchmarks.cs)

![alt text](/docs/benchmarks/temptable/imgs/temp-table.png)

## License
**EntityFrameworkCore.SqlServer.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
