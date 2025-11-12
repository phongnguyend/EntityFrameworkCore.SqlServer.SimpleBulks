using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class DbContextAsyncExtensions
{
    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNamesSelector)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this DbContext dbContext, T data, IReadOnlyCollection<string> columnNames, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNames)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleUpdateAsync(data, cancellationToken);
    }
}
