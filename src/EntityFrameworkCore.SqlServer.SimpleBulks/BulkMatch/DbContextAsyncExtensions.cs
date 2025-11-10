using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class DbContextAsyncExtensions
{
    public static Task<List<T>> BulkMatchAsync<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkMatchBuilder<T>()
       .WithReturnedColumns(table.PropertyNames)
         .WithTable(table)
     .WithMatchedColumns(matchedColumnsSelector)
      .WithBulkOptions(options)
      .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkMatchBuilder<T>()
     .WithReturnedColumns(returnedColumnsSelector)
        .WithTable(dbContext.GetTableInfor<T>())
    .WithMatchedColumns(matchedColumnsSelector)
    .WithBulkOptions(options)
   .ExecuteAsync(machedValues, cancellationToken);
    }
}
