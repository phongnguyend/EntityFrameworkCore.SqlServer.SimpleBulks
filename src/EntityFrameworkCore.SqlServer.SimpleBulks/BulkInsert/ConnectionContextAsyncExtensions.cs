using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
  .WithColumns(columnNamesSelector)
          .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
 .ExecuteAsync(data, cancellationToken);
    }


    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, SqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNames)
        .ToTable(table ?? TableMapper.Resolve<T>())
   .WithBulkOptions(options)
    .ExecuteAsync(data, cancellationToken);
    }

}