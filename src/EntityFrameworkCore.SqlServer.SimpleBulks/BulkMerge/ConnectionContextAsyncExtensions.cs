using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
     .WithId(idSelector)
      .WithUpdateColumns(updateColumnNamesSelector)
         .WithInsertColumns(insertColumnNamesSelector)
    .ToTable(TableMapper.Resolve(typeof(T)))
     .WithBulkOptions(options)
           .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
          .WithId(idColumns)
       .WithUpdateColumns(updateColumnNames)
   .WithInsertColumns(insertColumnNames)
   .ToTable(TableMapper.Resolve(typeof(T)))
           .WithBulkOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
         .WithId(idSelector)
       .WithUpdateColumns(updateColumnNamesSelector)
         .WithInsertColumns(insertColumnNamesSelector)
  .ToTable(table)
        .WithBulkOptions(options)
       .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
        .WithId(idColumns)
   .WithUpdateColumns(updateColumnNames)
     .WithInsertColumns(insertColumnNames)
     .ToTable(table)
   .WithBulkOptions(options)
 .ExecuteAsync(data, cancellationToken);
    }
}