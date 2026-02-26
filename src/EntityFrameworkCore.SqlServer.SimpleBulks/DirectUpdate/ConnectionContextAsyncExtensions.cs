using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(table.PrimaryKeys)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> columnNames, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(table.PrimaryKeys)
            .WithColumns(columnNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> keySelector, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keySelector)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleUpdateAsync(data, cancellationToken);
    }

    public static Task<BulkUpdateResult> DirectUpdateAsync<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> keys, IReadOnlyCollection<string> columnNames, BulkUpdateOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keys)
            .WithColumns(columnNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleUpdateAsync(data, cancellationToken);
    }
}