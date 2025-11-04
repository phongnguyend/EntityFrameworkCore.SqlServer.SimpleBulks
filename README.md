# EntityFrameworkCore.SqlServer.SimpleBulks
A very simple .net core library that can help to sync a large number of records in-memory into the database using the **SqlBulkCopy** class.
 
## Overview
This library provides extension methods so that you can use with your EntityFrameworkCore **DbContext** instance **DbContextExtensions.cs**
or you can use **ConnectionContextExtensions.cs** to work directly with a **SqlConnection** instance without using EntityFrameworkCore.

## Nuget
| Database | Package | GitHub |
| -------- | ------- | ------ |
| SQL Server| [EntityFrameworkCore.SqlServer.SimpleBulks](https://www.nuget.org/packages/EntityFrameworkCore.SqlServer.SimpleBulks) | [EntityFrameworkCore.SqlServer.SimpleBulks](https://github.com/phongnguyend/EntityFrameworkCore.SqlServer.SimpleBulks) |
| PostgreSQL| [EntityFrameworkCore.PostgreSQL.SimpleBulks](https://www.nuget.org/packages/EntityFrameworkCore.PostgreSQL.SimpleBulks) | [EntityFrameworkCore.PostgreSQL.SimpleBulks](https://github.com/phongnguyend/EntityFrameworkCore.PostgreSQL.SimpleBulks) |
| MySQL| [EntityFrameworkCore.MySQL.SimpleBulks](https://www.nuget.org/packages/EntityFrameworkCore.MySQL.SimpleBulks) | [EntityFrameworkCore.MySQL.SimpleBulks](https://github.com/phongnguyend/EntityFrameworkCore.MySQL.SimpleBulks) |

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
await dbct.BulkInsertAsync(rows);
await dbct.BulkInsertAsync(compositeKeyRows);

// Insert selected columns only
await dbct.BulkInsertAsync(rows,
    row => new { row.Column1, row.Column2, row.Column3 });
await dbct.BulkInsertAsync(compositeKeyRows,
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

await dbct.BulkUpdateAsync(rows,
    row => new { row.Column3, row.Column2 });
await dbct.BulkUpdateAsync(compositeKeyRows,
    row => new { row.Column3, row.Column2 });

await dbct.BulkMergeAsync(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 });
await dbct.BulkMergeAsync(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
await dbct.BulkDeleteAsync(rows);
await dbct.BulkDeleteAsync(compositeKeyRows);
```
### Using Dynamic String
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

await dbct.BulkUpdateAsync(rows,
    [ "Column3", "Column2" ]);
await dbct.BulkUpdateAsync(compositeKeyRows,
    [ "Column3", "Column2" ]);

await dbct.BulkMergeAsync(rows,
    ["Id"],
    [ "Column1", "Column2" ],
    [ "Column1", "Column2", "Column3" ]);
await dbct.BulkMergeAsync(compositeKeyRows,
    [ "Id1", "Id2" ],
    [ "Column1", "Column2", "Column3" ],
    [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);
```
### Using Builder Approach in case you need both Dynamic & Lambda Expression
```c#
await dbct.CreateBulkInsertBuilder<Row>()
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable(dbct.GetTableInfor(typeof(Row)))
	.ExecuteAsync(rows);
```

## ConnectionContextExtensions
### Using Lambda Expression
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

// Register Type - Table Name globaly
TableMapper.Register(typeof(Row), new SqlTableInfor("Rows"));
TableMapper.Register(typeof(CompositeKeyRow), new SqlTableInfor("CompositeKeyRows"));

var connection = new ConnectionContext(new SqlConnection(connectionString), null);

await connection.BulkInsertAsync(rows,
           row => new { row.Column1, row.Column2, row.Column3 });
await connection.BulkInsertAsync(compositeKeyRows,
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

await connection.BulkUpdateAsync(rows,
           row => row.Id,
           row => new { row.Column3, row.Column2 });
await connection.BulkUpdateAsync(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column3, row.Column2 });

await connection.BulkMergeAsync(rows,
           row => row.Id,
           row => new { row.Column1, row.Column2 },
           row => new { row.Column1, row.Column2, row.Column3 });
await connection.BulkMergeAsync(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column1, row.Column2, row.Column3 },
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
await connection.BulkDeleteAsync(rows, row => row.Id);
await connection.BulkDeleteAsync(compositeKeyRows, row => new { row.Id1, row.Id2 });
```
### Using Dynamic String
```c#
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

var connection = new ConnectionContext(new SqlConnection(connectionString), null);

await connection.BulkInsertAsync(rows,
           [ "Column1", "Column2", "Column3" ]);
await connection.BulkInsertAsync(compositeKeyRows,
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

await connection.BulkUpdateAsync(rows,
           ["Id"],
           [ "Column3", "Column2" ]);
await connection.BulkUpdateAsync(compositeKeyRows,
           [ "Id1", "Id2" ],
           [ "Column3", "Column2" ]);

await connection.BulkMergeAsync(rows,
           ["Id"],
           [ "Column1", "Column2" ],
           [ "Column1", "Column2", "Column3" ]);
await connection.BulkMergeAsync(compositeKeyRows,
           [ "Id1", "Id2" ],
           [ "Column1", "Column2", "Column3" ],
           [ "Id1", "Id2", "Column1", "Column2", "Column3" ]);

await connection.BulkDeleteAsync(rows, ["Id"]);
await connection.BulkDeleteAsync(compositeKeyRows, [ "Id1", "Id2" ]);
```
### Using Builder Approach in case you need both Dynamic & Lambda Expression
```c#
await connection.CreateBulkInsertBuilder<Row>()
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns([ "Column1", "Column2", "Column3" ])
	.WithOutputId(row => row.Id)
	// or .WithOutputId("Id")
	.ToTable(new SqlTableInfor("Rows"))
	.ExecuteAsync(rows);
```

## Execution Options
### BulkInsert
```c#
await _context.BulkInsertAsync(rows,
    row => new { row.Column1, row.Column2, row.Column3 },
    new BulkInsertOptions
    {
        KeepIdentity = false,
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### BulkUpdate
```c#
await _context.BulkUpdateAsync(rows,
    row => new { row.Column3, row.Column2 },
    new BulkUpdateOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### BulkDelete
```c#
await _context.BulkDeleteAsync(rows,
    new BulkDeleteOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### BulkMerge
```c#
await _context.BulkMergeAsync(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    new BulkMergeOptions
    {
        BatchSize = 0,
        Timeout = 30,
        WithHoldLock = false,
        ReturnDbGeneratedId = true,
        LogTo = Console.WriteLine
    });
```
### BulkMatch
```c#
var contactsFromDb = await _context.BulkMatchAsync(matchedContacts,
    x => new { x.CustomerId, x.CountryIsoCode },
    new BulkMatchOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### TempTable
```c#
var customerTableName = await _context.CreateTempTableAsync(customers,
    x => new
    {
        x.IdNumber,
        x.FirstName,
        x.LastName,
        x.CurrentCountryIsoCode
    },
    new TempTableOptions
    {
        BatchSize = 0,
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### DirectInsert
```c#
await _context.DirectInsertAsync(row,
    row => new { row.Column1, row.Column2, row.Column3 },
    new BulkInsertOptions
    {
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### DirectUpdate
```c#
await _context.DirectUpdateAsync(row,
    row => new { row.Column3, row.Column2 },
    new BulkUpdateOptions
    {
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```
### DirectDelete
```c#
await _context.DirectDeleteAsync(row,
    new BulkDeleteOptions
    {
        Timeout = 30,
        LogTo = Console.WriteLine
    });
```

## Returned Result
### BulkUpdate
```c#
var updateResult = await dbct.BulkUpdateAsync(rows, row => new { row.Column3, row.Column2 });

Console.WriteLine($"Updated: {updateResult.AffectedRows} row(s)");
```
### BulkDelete
```c#
var deleteResult = await dbct.BulkDeleteAsync(rows);

Console.WriteLine($"Deleted: {deleteResult.AffectedRows} row(s)");
```
### BulkMerge
```c#
var mergeResult = await dbct.BulkMergeAsync(rows,
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

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |        Gen1 |      Gen2 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|------------:|----------:|--------------:|
| **EFCoreInsert** |       **100** |     **45.19 ms** |    **NA** |           **-** |           **-** |         **-** |     **985.52 KB** |
|   BulkInsert |       100 |     32.68 ms |    NA |           - |           - |         - |      93.78 KB |
| **EFCoreInsert** |      **1000** |    **145.41 ms** |    **NA** |   **1000.0000** |           **-** |         **-** |     **9702.7 KB** |
|   BulkInsert |      1000 |     44.94 ms |    NA |           - |           - |         - |     573.84 KB |
| **EFCoreInsert** |     **10000** |    **788.90 ms** |    **NA** |  **14000.0000** |   **5000.0000** |         **-** |   **95727.38 KB** |
|   BulkInsert |     10000 |    126.36 ms |    NA |           - |           - |         - |    5320.53 KB |
| **EFCoreInsert** |    **100000** |  **7,107.29 ms** |    **NA** | **146000.0000** |  **36000.0000** |         **-** |  **950162.56 KB** |
|   BulkInsert |    100000 |    998.42 ms |    NA |   7000.0000 |   3000.0000 | 1000.0000 |   51730.81 KB |
| **EFCoreInsert** |    **250000** | **18,542.56 ms** |    **NA** | **365000.0000** |  **87000.0000** |         **-** | **2352262.34 KB** |
|   BulkInsert |    250000 |  2,576.88 ms |    NA |  16000.0000 |   5000.0000 | 1000.0000 |  125832.63 KB |
| **EFCoreInsert** |    **500000** | **34,957.34 ms** |    **NA** | **730000.0000** | **170000.0000** |         **-** | **4711772.88 KB** |
|   BulkInsert |    500000 |  5,553.61 ms |    NA |  30000.0000 |   9000.0000 | 1000.0000 |  252707.77 KB |


Multiple Tables (1x parent rows + 5x child rows) [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkInsertMultipleTablesBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |         Gen0 |        Gen1 |      Gen2 |     Allocated |
|------------- |---------- |-------------:|------:|-------------:|------------:|----------:|--------------:|
| **EFCoreInsert** |       **100** |    **226.22 ms** |    **NA** |    **1000.0000** |           **-** |         **-** |    **7438.51 KB** |
|   BulkInsert |       100 |     48.38 ms |    NA |            - |           - |         - |     444.18 KB |
| **EFCoreInsert** |      **1000** |    **566.95 ms** |    **NA** |   **11000.0000** |   **4000.0000** |         **-** |   **73518.48 KB** |
|   BulkInsert |      1000 |    125.77 ms |    NA |            - |           - |         - |    3460.21 KB |
| **EFCoreInsert** |     **10000** |  **6,268.42 ms** |    **NA** |  **114000.0000** |  **30000.0000** |         **-** |  **731076.92 KB** |
|   BulkInsert |     10000 |  1,066.74 ms |    NA |    5000.0000 |   2000.0000 | 1000.0000 |   33324.16 KB |
| **EFCoreInsert** |    **100000** | **59,389.89 ms** |    **NA** | **1138000.0000** | **264000.0000** |         **-** | **7282561.93 KB** |
|   BulkInsert |    100000 |  9,504.12 ms |    NA |   39000.0000 |  13000.0000 | 1000.0000 |  327100.08 KB |


### BulkUpdate
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkUpdateBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |       Gen1 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|-----------:|--------------:|
| **EFCoreUpdate** |       **100** |     **61.65 ms** |    **NA** |           **-** |          **-** |     **853.66 KB** |
|   BulkUpdate |       100 |     27.33 ms |    NA |           - |          - |      63.55 KB |
| **EFCoreUpdate** |      **1000** |    **143.39 ms** |    **NA** |   **1000.0000** |          **-** |     **8398.1 KB** |
|   BulkUpdate |      1000 |     43.95 ms |    NA |           - |          - |     379.25 KB |
| **EFCoreUpdate** |     **10000** |    **685.97 ms** |    **NA** |  **12000.0000** |  **3000.0000** |   **82396.19 KB** |
|   BulkUpdate |     10000 |    182.54 ms |    NA |           - |          - |    3499.74 KB |
| **EFCoreUpdate** |    **100000** |  **8,495.18 ms** |    **NA** | **120000.0000** | **28000.0000** |  **810248.07 KB** |
|   BulkUpdate |    100000 |  2,091.42 ms |    NA |   5000.0000 |  1000.0000 |   33819.46 KB |
| **EFCoreUpdate** |    **250000** | **17,859.49 ms** |    **NA** | **300000.0000** | **69000.0000** | **2005895.77 KB** |
|   BulkUpdate |    250000 |  4,290.07 ms |    NA |  13000.0000 |  7000.0000 |      84352 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|     Method | RowsCount |    Mean | Error |       Gen0 |       Gen1 | Allocated |
|----------- |---------- |--------:|------:|-----------:|-----------:|----------:|
| **BulkUpdate** |    **500000** | **10.19 s** |    **NA** | **27000.0000** | **16000.0000** | **164.63 MB** |
| **BulkUpdate** |   **1000000** | **17.03 s** |    **NA** | **54000.0000** | **37000.0000** | **329.12 MB** |


### BulkDelete
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkDeleteBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |       Gen0 |       Gen1 |    Allocated |
|------------- |---------- |-------------:|------:|-----------:|-----------:|-------------:|
| **EFCoreDelete** |       **100** |     **73.25 ms** |    **NA** |          **-** |          **-** |    **681.09 KB** |
|   BulkDelete |       100 |     29.42 ms |    NA |          - |          - |     43.45 KB |
| **EFCoreDelete** |      **1000** |    **176.83 ms** |    **NA** |  **1000.0000** |  **1000.0000** |      **6745 KB** |
|   BulkDelete |      1000 |     27.19 ms |    NA |          - |          - |    236.86 KB |
| **EFCoreDelete** |     **10000** |  **1,489.03 ms** |    **NA** | **10000.0000** |  **2000.0000** |  **66031.55 KB** |
|   BulkDelete |     10000 |    431.74 ms |    NA |          - |          - |   2150.99 KB |
| **EFCoreDelete** |     **20000** |  **6,084.87 ms** |    **NA** | **20000.0000** |  **7000.0000** |  **132403.3 KB** |
|   BulkDelete |     20000 |    276.52 ms |    NA |          - |          - |   4276.01 KB |
| **EFCoreDelete** |     **50000** | **39,933.60 ms** |    **NA** | **49000.0000** | **14000.0000** | **326164.25 KB** |
|   BulkDelete |     50000 |  1,477.09 ms |    NA |  1000.0000 |          - |  10594.63 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|     Method | RowsCount |       Mean | Error |       Gen0 |       Gen1 | Allocated |
|----------- |---------- |-----------:|------:|-----------:|-----------:|----------:|
| **BulkDelete** |    **100000** |   **937.7 ms** |    **NA** |  **3000.0000** |  **1000.0000** |  **20.67 MB** |
| **BulkDelete** |    **250000** | **2,619.7 ms** |    **NA** |  **7000.0000** |  **3000.0000** |   **51.7 MB** |
| **BulkDelete** |    **500000** | **4,897.7 ms** |    **NA** | **13000.0000** |  **6000.0000** | **103.22 MB** |
| **BulkDelete** |   **1000000** | **9,466.0 ms** |    **NA** | **26000.0000** | **12000.0000** | **206.28 MB** |


### BulkMerge
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |        Gen1 |      Gen2 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|------------:|----------:|--------------:|
| **EFCoreUpsert** |       **100** |     **82.11 ms** |    **NA** |           **-** |           **-** |         **-** |    **1840.23 KB** |
|    BulkMerge |       100 |     34.27 ms |    NA |           - |           - |         - |     162.96 KB |
| **EFCoreUpsert** |      **1000** |    **266.86 ms** |    **NA** |   **2000.0000** |   **1000.0000** |         **-** |   **17984.91 KB** |
|    BulkMerge |      1000 |     79.45 ms |    NA |           - |           - |         - |    1213.33 KB |
| **EFCoreUpsert** |     **10000** |  **1,451.20 ms** |    **NA** |  **26000.0000** |   **8000.0000** |         **-** |  **178385.15 KB** |
|    BulkMerge |     10000 |    677.47 ms |    NA |   1000.0000 |           - |         - |   11679.42 KB |
| **EFCoreUpsert** |    **100000** | **13,902.06 ms** |    **NA** | **266000.0000** |  **63000.0000** |         **-** | **1762696.52 KB** |
|    BulkMerge |    100000 |  3,415.31 ms |    NA |  16000.0000 |   6000.0000 | 1000.0000 |   115233.2 KB |
| **EFCoreUpsert** |    **250000** | **36,167.51 ms** |    **NA** | **665000.0000** | **152000.0000** |         **-** | **4362872.57 KB** |
|    BulkMerge |    250000 |  7,681.71 ms |    NA |  37000.0000 |  11000.0000 | 1000.0000 |  284187.09 KB |


[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeReturnDbGeneratedIdBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMergeReturnDbGeneratedIdBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|                 Method | RowsCount |        Mean | Error |       Gen0 |      Gen1 |      Gen2 |    Allocated |
|----------------------- |---------- |------------:|------:|-----------:|----------:|----------:|-------------:|
|    **ReturnDbGeneratedId** |       **100** |    **38.45 ms** |    **NA** |          **-** |         **-** |         **-** |    **151.09 KB** |
| NotReturnDbGeneratedId |       100 |    37.75 ms |    NA |          - |         - |         - |     116.8 KB |
|    **ReturnDbGeneratedId** |      **1000** |    **67.42 ms** |    **NA** |          **-** |         **-** |         **-** |   **1099.48 KB** |
| NotReturnDbGeneratedId |      1000 |    60.02 ms |    NA |          - |         - |         - |    769.23 KB |
|    **ReturnDbGeneratedId** |     **10000** |   **783.73 ms** |    **NA** |  **1000.0000** |         **-** |         **-** |  **10543.62 KB** |
| NotReturnDbGeneratedId |     10000 |   501.07 ms |    NA |  1000.0000 |         - |         - |   7348.79 KB |
|    **ReturnDbGeneratedId** |    **100000** | **3,187.89 ms** |    **NA** | **14000.0000** | **5000.0000** | **1000.0000** | **103878.09 KB** |
| NotReturnDbGeneratedId |    100000 | 2,741.31 ms |    NA | 11000.0000 | 5000.0000 | 1000.0000 |  72936.01 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|                 Method | RowsCount |     Mean | Error |        Gen0 |       Gen1 |      Gen2 |  Allocated |
|----------------------- |---------- |---------:|------:|------------:|-----------:|----------:|-----------:|
|    **ReturnDbGeneratedId** |    **250000** |  **7.799 s** |    **NA** |  **32000.0000** |  **8000.0000** |         **-** |   **249.8 MB** |
| NotReturnDbGeneratedId |    250000 |  6.619 s |    NA |  24000.0000 |  7000.0000 |         - |   177.7 MB |
|    **ReturnDbGeneratedId** |    **500000** | **15.051 s** |    **NA** |  **66000.0000** | **19000.0000** | **1000.0000** |  **500.64 MB** |
| NotReturnDbGeneratedId |    500000 | 14.328 s |    NA |  47000.0000 | 14000.0000 |         - |  355.19 MB |
|    **ReturnDbGeneratedId** |   **1000000** | **32.449 s** |    **NA** | **129000.0000** | **34000.0000** |         **-** | **1003.67 MB** |
| NotReturnDbGeneratedId |   1000000 | 28.253 s |    NA |  95000.0000 | 28000.0000 |         - |  710.22 MB |


### BulkMatch
Single Column [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchSingleColumnBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|            Method | RowsCount |          Mean | Error |        Gen0 |       Gen1 |      Gen2 |   Allocated |
|------------------ |---------- |--------------:|------:|------------:|-----------:|----------:|------------:|
|      **EFCoreSelect** |       **100** |     **97.373 ms** |    **NA** |           **-** |          **-** |         **-** |  **1008.33 KB** |
| EFCoreBatchSelect |       100 |      7.166 ms |    NA |           - |          - |         - |    94.77 KB |
|         BulkMatch |       100 |      8.570 ms |    NA |           - |          - |         - |   106.63 KB |
|      **EFCoreSelect** |      **1000** |    **720.250 ms** |    **NA** |   **1000.0000** |          **-** |         **-** |  **9761.42 KB** |
| EFCoreBatchSelect |      1000 |      6.375 ms |    NA |           - |          - |         - |   908.18 KB |
|         BulkMatch |      1000 |     15.445 ms |    NA |           - |          - |         - |   820.36 KB |
|      **EFCoreSelect** |     **10000** |  **8,075.686 ms** |    **NA** |  **15000.0000** |  **1000.0000** |         **-** | **97115.62 KB** |
| EFCoreBatchSelect |     10000 |     66.438 ms |    NA |   1000.0000 |          - |         - |  9092.91 KB |
|         BulkMatch |     10000 |     69.430 ms |    NA |   1000.0000 |          - |         - |  8177.76 KB |
|      **EFCoreSelect** |    **100000** | **81,088.718 ms** |    **NA** | **159000.0000** | **31000.0000** | **1000.0000** | **972204.7 KB** |
| EFCoreBatchSelect |    100000 |    920.412 ms |    NA |  11000.0000 |  4000.0000 | 1000.0000 | 91808.56 KB |
|         BulkMatch |    100000 |    742.030 ms |    NA |  13000.0000 |  6000.0000 | 1000.0000 | 82419.43 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|            Method | RowsCount |     Mean | Error |        Gen0 |       Gen1 |      Gen2 | Allocated |
|------------------ |---------- |---------:|------:|------------:|-----------:|----------:|----------:|
| **EFCoreBatchSelect** |    **250000** |  **2.101 s** |    **NA** |  **26000.0000** | **11000.0000** | **1000.0000** | **224.05 MB** |
|         BulkMatch |    250000 |  2.067 s |    NA |  32000.0000 | 12000.0000 | 1000.0000 | 201.64 MB |
| **EFCoreBatchSelect** |    **500000** |  **4.239 s** |    **NA** |  **53000.0000** | **20000.0000** | **2000.0000** | **448.85 MB** |
|         BulkMatch |    500000 |  4.507 s |    NA |  62000.0000 | 24000.0000 | 1000.0000 | 404.03 MB |
| **EFCoreBatchSelect** |   **1000000** |  **8.523 s** |    **NA** | **103000.0000** | **38000.0000** | **1000.0000** | **898.44 MB** |
|         BulkMatch |   1000000 | 11.585 s |    NA | 123000.0000 | 46000.0000 | 1000.0000 | 808.82 MB |


Multiple Columns [/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/BulkMatchMultipleColumnsBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|       Method | RowsCount |         Mean | Error |        Gen0 |       Gen1 |     Allocated |
|------------- |---------- |-------------:|------:|------------:|-----------:|--------------:|
| **EFCoreSelect** |       **100** |    **130.11 ms** |    **NA** |           **-** |          **-** |     **1256.8 KB** |
|    BulkMatch |       100 |     15.46 ms |    NA |           - |          - |     173.56 KB |
| **EFCoreSelect** |      **1000** |    **997.87 ms** |    **NA** |   **2000.0000** |          **-** |   **12373.85 KB** |
|    BulkMatch |      1000 |     43.35 ms |    NA |           - |          - |    1358.77 KB |
| **EFCoreSelect** |     **10000** |  **9,769.96 ms** |    **NA** |  **20000.0000** |  **4000.0000** |  **123595.97 KB** |
|    BulkMatch |     10000 |    238.80 ms |    NA |   2000.0000 |  1000.0000 |   13768.49 KB |
| **EFCoreSelect** |    **100000** | **89,204.16 ms** |    **NA** | **201000.0000** | **51000.0000** | **1237424.23 KB** |
|    BulkMatch |    100000 |  2,612.00 ms |    NA |  21000.0000 |  8000.0000 |  138686.52 KB |


``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|    Method | RowsCount |     Mean | Error |        Gen0 |       Gen1 |  Allocated |
|---------- |---------- |---------:|------:|------------:|-----------:|-----------:|
| **BulkMatch** |    **250000** |  **6.709 s** |    **NA** |  **53000.0000** | **19000.0000** |  **340.68 MB** |
| **BulkMatch** |    **500000** | **12.939 s** |    **NA** | **107000.0000** | **36000.0000** |  **683.46 MB** |
| **BulkMatch** |   **1000000** | **25.418 s** |    **NA** | **214000.0000** | **74000.0000** | **1369.34 MB** |


### TempTable
[/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/TempTableBenchmarks.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks/TempTableBenchmarks.cs)

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19045.5011)
11th Gen Intel Core i7-1165G7 2.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=8.0.400
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  Job-LGAVYD : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2

InvocationCount=1  IterationCount=1  UnrollFactor=1  
WarmupCount=0  

```
|          Method | RowsCount |         Mean | Error |       Gen0 |       Gen1 |      Gen2 |    Allocated |
|---------------- |---------- |-------------:|------:|-----------:|-----------:|----------:|-------------:|
| **CreateTempTable** |       **100** |     **7.639 ms** |    **NA** |          **-** |          **-** |         **-** |     **68.03 KB** |
| **CreateTempTable** |      **1000** |    **14.077 ms** |    **NA** |          **-** |          **-** |         **-** |    **373.76 KB** |
| **CreateTempTable** |     **10000** |    **89.789 ms** |    **NA** |          **-** |          **-** |         **-** |   **3455.46 KB** |
| **CreateTempTable** |    **100000** |   **574.937 ms** |    **NA** |  **4000.0000** |  **1000.0000** |         **-** |  **34081.95 KB** |
| **CreateTempTable** |    **250000** | **1,403.071 ms** |    **NA** | **12000.0000** |  **5000.0000** | **1000.0000** |  **85229.91 KB** |
| **CreateTempTable** |    **500000** | **2,838.562 ms** |    **NA** | **22000.0000** |  **8000.0000** | **1000.0000** | **170241.85 KB** |
| **CreateTempTable** |   **1000000** | **6,198.206 ms** |    **NA** | **43000.0000** | **14000.0000** | **1000.0000** |  **340282.7 KB** |


## License
**EntityFrameworkCore.SqlServer.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
