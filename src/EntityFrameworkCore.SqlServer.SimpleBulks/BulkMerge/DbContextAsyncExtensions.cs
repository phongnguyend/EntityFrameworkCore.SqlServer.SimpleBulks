using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class DbContextAsyncExtensions
{
    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return dbContext.CreateBulkMergeBuilder<T>()
       .WithId(idSelector)
     .WithUpdateColumns(updateColumnNamesSelector)
        .WithInsertColumns(insertColumnNamesSelector)
      .WithOutputId(outputIdColumn)
      .ToTable(dbContext.GetTableInfor(typeof(T)))
        .WithBulkOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return dbContext.CreateBulkMergeBuilder<T>()
          .WithId(idColumns)
         .WithUpdateColumns(updateColumnNames)
       .WithInsertColumns(insertColumnNames)
       .WithOutputId(outputIdColumn)
          .ToTable(dbContext.GetTableInfor(typeof(T)))
         .WithBulkOptions(options)
              .ExecuteAsync(data, cancellationToken);
    }
}
