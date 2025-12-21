using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this DbContext dbContext, T data, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(table.PrimaryKeys)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleDeleteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this DbContext dbContext, T data, Expression<Func<T, object>> keySelector, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(keySelector)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleDeleteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this DbContext dbContext, T data, IReadOnlyCollection<string> keys, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(keys)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleDeleteAsync(data, cancellationToken);
    }
}
