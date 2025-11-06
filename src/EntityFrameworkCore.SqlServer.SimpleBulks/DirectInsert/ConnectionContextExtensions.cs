using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class ConnectionContextExtensions
{
    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
      .WithColumns(columnNamesSelector)
        .ToTable(table ?? TableMapper.Resolve<T>())
.WithBulkOptions(options)
    .SingleInsert(data);
    }


    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, SqlTableInfor table = null, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
.WithColumns(columnNames)
  .ToTable(table ?? TableMapper.Resolve<T>())
     .WithBulkOptions(options)
   .SingleInsert(data);
    }

}