using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, SqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
         .WithId(idSelector)
    .ToTable(table ?? TableMapper.Resolve<T>())
         .WithBulkOptions(options)
           .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, SqlTableInfor table = null, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
       .WithId(idColumns)
       .ToTable(table ?? TableMapper.Resolve<T>())
              .WithBulkOptions(options)
           .SingleDelete(data);
    }
}