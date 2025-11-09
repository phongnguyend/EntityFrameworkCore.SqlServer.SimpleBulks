using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class ConnectionContextExtensions
{
    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
  .WithId(temp.PrimaryKeys)
   .WithColumns(columnNamesSelector)
      .ToTable(temp)
  .WithBulkOptions(options)
  .SingleUpdate(data);
    }

    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> columnNames, SqlTableInfor table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
       .WithId(temp.PrimaryKeys)
            .WithColumns(columnNames)
       .ToTable(temp)
           .WithBulkOptions(options)
          .SingleUpdate(data);
    }
}