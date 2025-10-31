using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public static class DbContextAsyncExtensions
{
    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new TempTableBuilder<T>(connectionContext)
           .WithColumns(columnNamesSelector)
  .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
        .WithTempTableOptions(options)
      .ExecuteAsync(data, cancellationToken);
    }

    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new TempTableBuilder<T>(connectionContext)
           .WithColumns(columnNames)
               .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
        .WithTempTableOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }
}
