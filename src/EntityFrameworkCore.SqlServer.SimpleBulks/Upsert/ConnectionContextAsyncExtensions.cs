using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Upsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, SqlTableInfor<T> table = null, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
 .WithId(idSelector)
    .WithUpdateColumns(updateColumnNamesSelector)
  .WithInsertColumns(insertColumnNamesSelector)
  .ToTable(table ?? TableMapper.Resolve<T>())
      .WithBulkOptions(options)
     .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, SqlTableInfor<T> table = null, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
    .WithId(idColumns)
 .WithUpdateColumns(updateColumnNames)
 .WithInsertColumns(insertColumnNames)
      .ToTable(table ?? TableMapper.Resolve<T>())
    .WithBulkOptions(options)
        .SingleMergeAsync(data, cancellationToken);
    }
}