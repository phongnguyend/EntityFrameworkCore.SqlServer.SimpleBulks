using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
  .WithColumns(columnNamesSelector)
          .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
 .SingleInsertAsync(data, cancellationToken);
    }

    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, SqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
          .WithColumns(columnNamesSelector)
             .ToTable(table ?? TableMapper.Resolve<T>())
               .WithOutputId(idSelector)
            .WithBulkOptions(options)
         .SingleInsertAsync(data, cancellationToken);
    }

    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, SqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNames)
        .ToTable(table ?? TableMapper.Resolve<T>())
   .WithBulkOptions(options)
    .SingleInsertAsync(data, cancellationToken);
    }

    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, string idColumnName, SqlTableInfor table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
          .WithColumns(columnNames)
            .ToTable(table ?? TableMapper.Resolve<T>())
         .WithOutputId(idColumnName)
    .WithBulkOptions(options)
        .SingleInsertAsync(data, cancellationToken);
    }
}