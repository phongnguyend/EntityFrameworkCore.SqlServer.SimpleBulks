using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class DbContextAsyncExtensions
{
    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkMergeBuilder<T>()
       .WithId(idSelector)
     .WithUpdateColumns(updateColumnNamesSelector)
        .WithInsertColumns(insertColumnNamesSelector)
      .ToTable(dbContext.GetTableInfor<T>())
        .WithBulkOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkMergeBuilder<T>()
          .WithId(idColumns)
         .WithUpdateColumns(updateColumnNames)
       .WithInsertColumns(insertColumnNames)
          .ToTable(dbContext.GetTableInfor<T>())
         .WithBulkOptions(options)
              .ExecuteAsync(data, cancellationToken);
    }
}
