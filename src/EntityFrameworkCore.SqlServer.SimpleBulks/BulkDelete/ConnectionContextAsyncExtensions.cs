using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
     .WithId(idSelector)
        .ToTable(TableMapper.Resolve<T>())
.WithBulkOptions(options)
  .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
              .WithId(idColumns)
    .ToTable(TableMapper.Resolve<T>())
   .WithBulkOptions(options)
   .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
     .WithId(idSelector)
          .ToTable(table)
        .WithBulkOptions(options)
          .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
  .WithId(idColumns)
       .ToTable(table)
            .WithBulkOptions(options)
 .ExecuteAsync(data, cancellationToken);
    }
}