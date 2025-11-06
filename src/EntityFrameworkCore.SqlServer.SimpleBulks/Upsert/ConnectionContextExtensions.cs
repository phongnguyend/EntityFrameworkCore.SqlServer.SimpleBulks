using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Upsert;

public static class ConnectionContextExtensions
{
    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Expression<Func<T, object>> outputIdSelector = null, SqlTableInfor table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
         .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
           .WithInsertColumns(insertColumnNamesSelector)
           .WithOutputId(outputIdSelector)
           .ToTable(table ?? TableMapper.Resolve<T>())
             .WithBulkOptions(options)
             .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, string outputId = null, SqlTableInfor table = null, BulkMergeOptions options = null)
    {
        return connectionContext.CreateBulkMergeBuilder<T>()
      .WithId(idColumns)
          .WithUpdateColumns(updateColumnNames)
    .WithInsertColumns(insertColumnNames)
    .WithOutputId(outputId)
       .ToTable(table ?? TableMapper.Resolve<T>())
  .WithBulkOptions(options)
          .SingleMerge(data);
    }
}