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
    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateTempTableBuilder<T>()
    .WithColumns(columnNamesSelector)
    .WithMappingContext(dbContext.GetMappingContext<T>())
        .WithTempTableOptions(options)
      .ExecuteAsync(data, cancellationToken);
    }

    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateTempTableBuilder<T>()
      .WithColumns(columnNames)
      .WithMappingContext(dbContext.GetMappingContext<T>())
        .WithTempTableOptions(options)
    .ExecuteAsync(data, cancellationToken);
    }
}
