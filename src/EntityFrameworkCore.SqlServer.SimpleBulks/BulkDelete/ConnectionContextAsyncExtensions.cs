using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(table.PrimaryKeys)
            .ToTable(table)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> keySelector, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keySelector)
            .ToTable(table)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> keys, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keys)
            .ToTable(table)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }
}