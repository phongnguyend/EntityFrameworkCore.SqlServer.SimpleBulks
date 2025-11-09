using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public static class ConnectionContextAsyncExtensions
{
    public static Task<string> CreateTempTableAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateTempTableBuilder<T>()
            .WithColumns(columnNamesSelector)
            .WithMappingContext(typeof(T).GetMappingContext())
            .WithTempTableOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<string> CreateTempTableAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, TempTableOptions options = null, CancellationToken cancellationToken = default)
    {
        return connectionContext.CreateTempTableBuilder<T>()
            .WithColumns(columnNames)
            .WithMappingContext(typeof(T).GetMappingContext())
            .WithTempTableOptions(options)
            .ExecuteAsync(data, cancellationToken);
    }
}
