using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class DbContextExtensions
{
    public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null)
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
         .Execute(data);
    }

    public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null)
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
      .Execute(data);
    }
}
