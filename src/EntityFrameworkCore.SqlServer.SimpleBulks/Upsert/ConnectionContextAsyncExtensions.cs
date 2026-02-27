using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Upsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        if (options?.ConfigureWhenNotMatchedBySource is not null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return connectionContext.CreateBulkMergeBuilder<T>()
            .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
            .WithInsertColumns(insertColumnNamesSelector)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> idColumns, IReadOnlyCollection<string> updateColumnNames, IReadOnlyCollection<string> insertColumnNames, BulkMergeOptions options = null, CancellationToken cancellationToken = default)
    {
        if (options?.ConfigureWhenNotMatchedBySource is not null)
        {
            throw new ArgumentException($"{nameof(BulkMergeOptions.ConfigureWhenNotMatchedBySource)} is not supported for Upsert operations.", nameof(options));
        }

        return connectionContext.CreateBulkMergeBuilder<T>()
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .SingleMergeAsync(data, cancellationToken);
    }
}