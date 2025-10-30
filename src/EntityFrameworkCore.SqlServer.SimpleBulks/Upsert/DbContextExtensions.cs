using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Upsert;

public static class DbContextExtensions
{
    public static BulkMergeResult Upsert<T>(this DbContext dbContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(connectionContext)
       .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
      .WithInsertColumns(insertColumnNamesSelector)
            .WithOutputId(outputIdColumn)
         .ToTable(dbContext.GetTableInfor(typeof(T)))
            .ConfigureBulkOptions(configureOptions)
            .SingleMerge(data);
    }

    public static BulkMergeResult Upsert<T>(this DbContext dbContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var outputIdColumn = dbContext.GetOutputId(typeof(T))?.PropertyName;

        return new BulkMergeBuilder<T>(connectionContext)
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
        .WithOutputId(outputIdColumn)
  .ToTable(dbContext.GetTableInfor(typeof(T)))
     .ConfigureBulkOptions(configureOptions)
            .SingleMerge(data);
    }
}
