# EntityFrameworkCore.SqlServer.SimpleBulks
This is a very simple .net core library that can help to work with large number of records using the SqlBulkCopy class.
 
## Overview
This library provides extension methods so that you can use with your EntityFrameworkCore DbContext instance:
[DbContextExtensions.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks/Extensions/DbContextExtensions.cs)
or you can use [SqlConnectionExtensions.cs](/src/EntityFrameworkCore.SqlServer.SimpleBulks/Extensions/SqlConnectionExtensions.cs) to work directly with a SqlConnection instance.

## EntityFrameworkCore.SqlServer.SimpleBulks supports:
* Bulk Insert
* Bulk Update
* Bulk Delete

## Examples
[EntityFrameworkCore.SqlServer.SimpleBulks.Demo](/src/EntityFrameworkCore.SqlServer.SimpleBulks.Demo/Program.cs)

## License
[MIT ](/LICENSE)
