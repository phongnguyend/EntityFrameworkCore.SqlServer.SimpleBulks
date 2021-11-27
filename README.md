# EntityFrameworkCore.SqlServer.SimpleBulks
This is a very simple .net core library that can help to work with large number of records using the **SqlBulkCopy** class.
 
## Overview
This library provides extension methods so that you can use with your EntityFrameworkCore **DbContext** instance:
[DbContextExtensions.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks/Extensions/DbContextExtensions.cs)
or you can use [SqlConnectionExtensions.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks/Extensions/SqlConnectionExtensions.cs) to work directly with a **SqlConnection** instance.

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
### Using Dynamic String:
```c#
dbct.BulkInsert(rows, "Rows",
    new string[] { "Column1", "Column2", "Column3" });
dbct.BulkInsert(rows.Take(1000), "Rows",
    typeof(Row).GetDbColumnNames("Id"));
dbct.BulkInsert(compositeKeyRows, "CompositeKeyRows",
    new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });

dbct.BulkUpdate(rows, "Rows",
    "Id",
    new string[] { "Column3", "Column2" });
dbct.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
    new string[] { "Id1", "Id2" },
    new string[] { "Column3", "Column2" });

dbct.BulkMerge(rows, "Rows",
    "Id",
    new string[] { "Column1", "Column2" },
    new string[] { "Column1", "Column2", "Column3" });
dbct.BulkMerge(compositeKeyRows, "CompositeKeyRows",
    new string[] { "Id1", "Id2" },
    new string[] { "Column1", "Column2", "Column3" },
    new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });

dbct.BulkDelete(rows, "Rows", "Id");
dbct.BulkDelete(compositeKeyRows, "CompositeKeyRows", new List<string> { "Id1", "Id2" });
```

### Using Lambda Expression:
```c#
dbct.BulkInsert(rows,
    row => new { row.Column1, row.Column2, row.Column3 });
dbct.BulkInsert(compositeKeyRows,
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });

dbct.BulkUpdate(rows,
    row => row.Id,
    row => new { row.Column3, row.Column2 });
dbct.BulkUpdate(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column3, row.Column2 });

dbct.BulkMerge(rows,
    row => row.Id,
    row => new { row.Column1, row.Column2 },
    row => new { row.Column1, row.Column2, row.Column3 });
dbct.BulkMerge(compositeKeyRows,
    row => new { row.Id1, row.Id2 },
    row => new { row.Column1, row.Column2, row.Column3 },
    row => new { row.Id1, row.Id2, row.Column1, row.Column2, row.Column3 });
                        
dbct.BulkDelete(rows, row => row.Id);
dbct.BulkDelete(compositeKeyRows, row => new { row.Id1, row.Id2 });
```

## SqlConnectionExtensions:
### Using Dynamic String:
```c#
connection.BulkInsert(rows, "Rows",
           new string[] { "Column1", "Column2", "Column3" });
connection.BulkInsert(rows.Take(1000), "Rows",
           typeof(Row).GetDbColumnNames("Id"));
connection.BulkInsert(compositeKeyRows, "CompositeKeyRows",
           new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });

connection.BulkUpdate(rows, "Rows",
           "Id",
           new string[] { "Column3", "Column2" });
connection.BulkUpdate(compositeKeyRows, "CompositeKeyRows",
           new string[] { "Id1", "Id2" },
           new string[] { "Column3", "Column2" });

connection.BulkMerge(rows, "Rows",
           "Id",
           new string[] { "Column1", "Column2" },
           new string[] { "Column1", "Column2", "Column3" });
connection.BulkMerge(compositeKeyRows, "CompositeKeyRows",
           new string[] { "Id1", "Id2" },
           new string[] { "Column1", "Column2", "Column3" },
           new string[] { "Id1", "Id2", "Column1", "Column2", "Column3" });

connection.BulkDelete(rows, "Rows", "Id");
connection.BulkDelete(compositeKeyRows, "CompositeKeyRows", new List<string> { "Id1", "Id2" });
```

### Using Lambda Expression:
```c#
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

## License
**EntityFrameworkCore.SqlServer.SimpleBulks** is licensed under the [MIT](/LICENSE) license.
