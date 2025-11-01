using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
         .WithId(idSelector)
    .ToTable(TableMapper.Resolve(typeof(T)))
         .WithBulkOptions(options)
           .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
       .WithId(idColumns)
       .ToTable(TableMapper.Resolve(typeof(T)))
              .WithBulkOptions(options)
           .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
      .WithId(idSelector)
        .ToTable(table)
       .WithBulkOptions(options)
        .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, BulkDeleteOptions options = null)
    {
        return connectionContext.CreateBulkDeleteBuilder<T>()
   .WithId(idColumns)
            .ToTable(table)
        .WithBulkOptions(options)
      .Execute(data);
    }
}