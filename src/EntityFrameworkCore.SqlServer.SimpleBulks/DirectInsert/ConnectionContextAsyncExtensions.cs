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
    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, SqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(temp.InsertablePropertyNames)
       .ToTable(temp)
          .WithBulkOptions(options)
            .SingleInsertAsync(data, cancellationToken);
    }

    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
       .WithColumns(columnNamesSelector)
       .ToTable(table ?? TableMapper.Resolve<T>())
          .WithBulkOptions(options)
            .SingleInsertAsync(data, cancellationToken);
    }


    public static Task DirectInsertAsync<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> columnNames, SqlTableInfor<T> table = null, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
   .WithColumns(columnNames)
  .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
 .SingleInsertAsync(data, cancellationToken);
    }
}