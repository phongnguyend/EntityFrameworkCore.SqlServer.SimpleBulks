using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class DbContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkInsertBuilder<T>()
              .WithColumns(table.InsertablePropertyNames)
              .ToTable(table)
              .WithBulkOptions(options)
              .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkInsertBuilder<T>()
              .WithColumns(columnNamesSelector)
              .ToTable(dbContext.GetTableInfor<T>())
              .WithBulkOptions(options)
              .ExecuteAsync(data, cancellationToken);
    }
}
