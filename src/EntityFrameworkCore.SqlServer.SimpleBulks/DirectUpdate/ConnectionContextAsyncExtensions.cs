using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
      .WithId(idSelector)
       .WithColumns(columnNamesSelector)
     .ToTable(table ?? TableMapper.Resolve<T>())
 .WithBulkOptions(options)
.SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, SqlTableInfor table = null, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
                 .WithId(idColumns)
              .WithColumns(columnNames)
               .ToTable(table ?? TableMapper.Resolve<T>())
            .WithBulkOptions(options)
             .SingleUpdateAsync(data, cancellationToken);
    }
}