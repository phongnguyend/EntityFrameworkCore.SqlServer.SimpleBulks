using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class ConnectionContextAsyncExtensions
{
    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(table.PropertyNames)
            .WithTable(table)
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
            .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
     .WithReturnedColumns(returnedColumnsSelector)
         .WithTable(TableMapper.Resolve<T>(options))
             .WithMatchedColumns(matchedColumnsSelector)
        .WithBulkOptions(options)
        .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, IReadOnlyCollection<string> matchedColumns, IReadOnlyCollection<string> returnedColumns, BulkMatchOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
        .WithReturnedColumns(returnedColumns)
        .WithTable(TableMapper.Resolve<T>(options))
          .WithMatchedColumns(matchedColumns)
        .WithBulkOptions(options)
        .ExecuteAsync(machedValues, cancellationToken);
    }
}