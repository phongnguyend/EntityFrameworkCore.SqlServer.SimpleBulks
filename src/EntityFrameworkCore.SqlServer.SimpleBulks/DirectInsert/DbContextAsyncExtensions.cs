using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class DbContextAsyncExtensions
{
    public static Task DirectInsertAsync<T>(this DbContext dbContext, T data, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkInsertBuilder<T>()
          .WithColumns(dbContext.GetInsertablePropertyNames(typeof(T)))
       .ToTable(dbContext.GetTableInfor(typeof(T)))
             .WithBulkOptions(options)
           .SingleInsertAsync(data, cancellationToken);
    }

    public static Task DirectInsertAsync<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkInsertBuilder<T>()
      .WithColumns(columnNamesSelector)
 .ToTable(dbContext.GetTableInfor(typeof(T)))
  .WithBulkOptions(options)
   .SingleInsertAsync(data, cancellationToken);
    }
}
