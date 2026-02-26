using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkInsertBuilder<T>()
            .WithColumns(table.InsertablePropertyNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
            .WithColumns(columnNamesSelector)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkInsertBuilder<T>()
            .WithColumns(columnNames)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }
}