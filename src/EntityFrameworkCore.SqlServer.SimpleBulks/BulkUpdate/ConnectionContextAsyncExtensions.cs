using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkUpdateBuilder<T>(connectionContext)
   .WithId(idSelector)
     .WithColumns(columnNamesSelector)
   .ToTable(table)
  .WithBulkOptions(options)
     .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkUpdateBuilder<T>(connectionContext)
   .WithId(idColumns)
       .WithColumns(columnNames)
    .ToTable(table)
   .WithBulkOptions(options)
   .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkUpdateBuilder<T>(connectionContext)
      .WithId(idSelector)
  .WithColumns(columnNamesSelector)
.ToTable(table)
  .WithBulkOptions(options)
 .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, IEnumerable<string> columnNames, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkUpdateBuilder<T>(connectionContext)
 .WithId(idColumns)
  .WithColumns(columnNames)
     .ToTable(table)
      .WithBulkOptions(options)
     .ExecuteAsync(data, cancellationToken);
    }
}