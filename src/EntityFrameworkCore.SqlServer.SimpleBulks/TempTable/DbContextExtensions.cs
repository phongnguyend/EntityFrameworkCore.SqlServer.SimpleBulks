using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public static class DbContextExtensions
{
    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, TempTableOptions options = null)
    {
        return new TempTableBuilder<T>(dbContext.GetConnectionContext())
         .WithColumns(columnNamesSelector)
          .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
             .WithTempTableOptions(options)
              .Execute(data);
    }

    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, TempTableOptions options = null)
    {
        return new TempTableBuilder<T>(dbContext.GetConnectionContext())
         .WithColumns(columnNames)
          .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
         .WithTempTableOptions(options)
          .Execute(data);
    }
}
