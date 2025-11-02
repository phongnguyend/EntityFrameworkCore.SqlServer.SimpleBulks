using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, SqlTableInfor table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
     .WithId(idSelector)
        .ToTable(table ?? TableMapper.Resolve<T>())
.WithBulkOptions(options)
  .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, SqlTableInfor table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
              .WithId(idColumns)
    .ToTable(table ?? TableMapper.Resolve<T>())
   .WithBulkOptions(options)
   .ExecuteAsync(data, cancellationToken);
    }
}