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
    row => new { row.Column1, row.Column2, row.Column3 });
dbct.BulkMerge(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
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
    new [] { "Column1", "Column2", "Column3" });
dbct.BulkMerge(compositeKeyRows,
    new [] { "Id1", "Id2" },
    new [] { "Column1", "Column2", "Column3" },
    new [] { "Id1", "Id2", "Column1", "Column2", "Column3" });
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression:
```c#
new BulkInsertBuilder<Row>(dbct.GetSqlConnection())
	.WithData(rows)
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns(new [] { "Column1", "Column2", "Column3" })
	.WithOuputId(row => row.Id)
	// or .WithOuputId("Id")
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
           row => new { row.Column1, row.Column2, row.Column3 });
connection.BulkMerge(compositeKeyRows,
           row => new { row.Id1, row.Id2 },
           row => new { row.Column1, row.Column2, row.Column3 },
           row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
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
           new [] { "Column1", "Column2", "Column3" });
connection.BulkMerge(compositeKeyRows, "CompositeKeyRows",
           new [] { "Id1", "Id2" },
           new [] { "Column1", "Column2", "Column3" },
           new [] { "Id1", "Id2", "Column1", "Column2", "Column3" });

connection.BulkDelete(rows, "Rows", "Id");
connection.BulkDelete(compositeKeyRows, "CompositeKeyRows", new [] { "Id1", "Id2" });
```
### Using Builder Approach in case you need to mix both Dynamic & Lambda Expression:
```c#
new BulkInsertBuilder<Row>(connection)
	.WithData(rows)
	.WithColumns(row => new { row.Column1, row.Column2, row.Column3 })
	// or .WithColumns(new [] { "Column1", "Column2", "Column3" })
	.WithOuputId(row => row.Id)
	// or .WithOuputId("Id")
	.ToTable("Rows")
	.Execute();
```

## License
**EntityFrameworkCore.SqlServer.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
