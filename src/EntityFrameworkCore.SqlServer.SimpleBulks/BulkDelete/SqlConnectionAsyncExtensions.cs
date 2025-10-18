using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class SqlConnectionAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkDeleteBuilder<T>(connection)
              .WithId(idSelector)
              .ToTable(table)
              .ConfigureBulkOptions(configureOptions)
              .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkDeleteBuilder<T>(connection)
            .WithId(idColumn)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkDeleteBuilder<T>(connection)
            .WithId(idColumns)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this SqlConnection connection, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkDeleteBuilder<T>(connection)
            .WithId(idSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this SqlConnection connection, IEnumerable<T> data, TableInfor table, string idColumn, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkDeleteBuilder<T>(connection)
            .WithId(idColumn)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this SqlConnection connection, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, Action<BulkDeleteOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkDeleteBuilder<T>(connection)
            .WithId(idColumns)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }
}
