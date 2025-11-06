using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Expression<Func<T, object>> outputIdSelector = null, SqlTableInfor table = null, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
     .WithId(idSelector)
      .WithUpdateColumns(updateColumnNamesSelector)
         .WithInsertColumns(insertColumnNamesSelector)
         .WithOutputId(outputIdSelector)
    .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
           .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, string outputId = null, SqlTableInfor table = null, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
          .WithId(idColumns)
       .WithUpdateColumns(updateColumnNames)
   .WithInsertColumns(insertColumnNames)
   .WithOutputId(outputId)
   .ToTable(table ?? TableMapper.Resolve<T>())
           .WithBulkOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }
}