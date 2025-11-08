using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public static class DbContextAsyncExtensions
{
    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));

        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNamesSelector)
             .ToTable(table)
             .WithBulkOptions(options)
             .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> BulkUpdateAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));

        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNames)
             .ToTable(table)
             .WithBulkOptions(options)
             .ExecuteAsync(data, cancellationToken);
    }
}
