using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class DbContextExtensions
{
    public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null)
    {
        return dbContext.CreateBulkMergeBuilder<T>()
      .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
              .WithInsertColumns(insertColumnNamesSelector)
               .ToTable(dbContext.GetTableInfor(typeof(T)))
                .WithBulkOptions(options)
         .Execute(data);
    }

    public static BulkMergeResult BulkMerge<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null)
    {
        return dbContext.CreateBulkMergeBuilder<T>()
           .WithId(idColumns)
        .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
        .WithBulkOptions(options)
           .Execute(data);
    }
}
