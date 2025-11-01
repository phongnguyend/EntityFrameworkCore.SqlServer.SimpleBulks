using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class ConnectionContextExtensions
{
    public static BulkMergeResult BulkMerge<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
       .WithId(idSelector)
     .WithUpdateColumns(updateColumnNamesSelector)
          .WithInsertColumns(insertColumnNamesSelector)
.ToTable(TableMapper.Resolve(typeof(T)))
  .WithBulkOptions(options)
         .Execute(data);
    }

    public static BulkMergeResult BulkMerge<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
           .WithId(idColumns)
        .WithUpdateColumns(updateColumnNames)
       .WithInsertColumns(insertColumnNames)
         .ToTable(TableMapper.Resolve(typeof(T)))
              .WithBulkOptions(options)
        .Execute(data);
    }

    public static BulkMergeResult BulkMerge<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
       .WithId(idSelector)
          .WithUpdateColumns(updateColumnNamesSelector)
    .WithInsertColumns(insertColumnNamesSelector)
      .ToTable(table)
        .WithBulkOptions(options)
         .Execute(data);
    }

    public static BulkMergeResult BulkMerge<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
               .WithId(idColumns)
         .WithUpdateColumns(updateColumnNames)
       .WithInsertColumns(insertColumnNames)
         .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }
}