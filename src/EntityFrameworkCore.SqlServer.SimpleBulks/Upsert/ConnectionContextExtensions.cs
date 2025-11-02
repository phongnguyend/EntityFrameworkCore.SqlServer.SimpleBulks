using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Upsert;

public static class ConnectionContextExtensions
{
    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
         .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
           .WithInsertColumns(insertColumnNamesSelector)
           .ToTable(TableMapper.Resolve<T>())
             .WithBulkOptions(options)
             .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
      .WithId(idColumns)
          .WithUpdateColumns(updateColumnNames)
    .WithInsertColumns(insertColumnNames)
       .ToTable(TableMapper.Resolve<T>())
  .WithBulkOptions(options)
          .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, TableInfor table, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
      .WithId(idSelector)
    .WithUpdateColumns(updateColumnNamesSelector)
   .WithInsertColumns(insertColumnNamesSelector)
      .ToTable(table)
       .WithBulkOptions(options)
       .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, TableInfor table, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
      .WithId(idColumns)
   .WithUpdateColumns(updateColumnNames)
          .WithInsertColumns(insertColumnNames)
        .ToTable(table)
   .WithBulkOptions(options)
 .SingleMerge(data);
    }
}