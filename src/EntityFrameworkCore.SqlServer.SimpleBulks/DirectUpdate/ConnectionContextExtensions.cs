using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class ConnectionContextExtensions
{
    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor table = null, BulkUpdateOptions options = null)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
  .WithId(idSelector)
      .WithColumns(columnNamesSelector)
       .ToTable(table ?? TableMapper.Resolve<T>())
          .WithBulkOptions(options)
   .SingleUpdate(data);
    }

    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, SqlTableInfor table = null, BulkUpdateOptions options = null)
    {
        return connectionContext.CreateBulkUpdateBuilder<T>()
           .WithId(idColumns)
         .WithColumns(columnNames)
                  .ToTable(table ?? TableMapper.Resolve<T>())
            .WithBulkOptions(options)
            .SingleUpdate(data);
    }
}