using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Upsert;

public static class SqlConnectionAsyncExtensions
{
    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMergeBuilder<T>(connectionContext.Connection)
            .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
            .WithInsertColumns(insertColumnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMergeBuilder<T>(connectionContext.Connection)
            .WithId(idColumn)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMergeBuilder<T>(connectionContext.Connection)
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, TableInfor table, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMergeBuilder<T>(connectionContext.Connection)
            .WithId(idSelector)
            .WithUpdateColumns(updateColumnNamesSelector)
            .WithInsertColumns(insertColumnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, TableInfor table, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMergeBuilder<T>(connectionContext.Connection)
            .WithId(idColumn)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .SingleMergeAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> UpsertAsync<T>(this ConnectionContext connectionContext, T data, TableInfor table, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMergeBuilder<T>(connectionContext.Connection)
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .SingleMergeAsync(data, cancellationToken);
    }
}
