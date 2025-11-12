using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class DbContextExtensions
{
    public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        return dbContext.CreateBulkMergeBuilder<T>()
      .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
              .WithInsertColumns(insertColumnNamesSelector)
               .ToTable(dbContext.GetTableInfor<T>())
                .WithBulkOptions(options)
         .Execute(data);
    }

    public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> idColumns, IReadOnlyCollection<string> updateColumnNames, IReadOnlyCollection<string> insertColumnNames, BulkMergeOptions options = null)
    {
        return dbContext.CreateBulkMergeBuilder<T>()
           .WithId(idColumns)
        .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(dbContext.GetTableInfor<T>())
        .WithBulkOptions(options)
           .Execute(data);
    }
}
