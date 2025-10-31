using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMergeBuilder<T>(connectionContext)
 .WithId(idSelector)
         .WithUpdateColumns(updateColumnNamesSelector)
   .WithInsertColumns(insertColumnNamesSelector)
    .ToTable(table)
       .ConfigureBulkOptions(configureOptions)
           .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMergeBuilder<T>(connectionContext)
     .WithId(idColumns)
     .WithUpdateColumns(updateColumnNames)
           .WithInsertColumns(insertColumnNames)
                .ToTable(table)
       .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMergeBuilder<T>(connectionContext)
      .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
  .WithInsertColumns(insertColumnNamesSelector)
         .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
     .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMergeBuilder<T>(connectionContext)
      .WithId(idColumns)
    .WithUpdateColumns(updateColumnNames)
       .WithInsertColumns(insertColumnNames)
  .ToTable(table)
         .ConfigureBulkOptions(configureOptions)
    .ExecuteAsync(data, cancellationToken);
    }
}