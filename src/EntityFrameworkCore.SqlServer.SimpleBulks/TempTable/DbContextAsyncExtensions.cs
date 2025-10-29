using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public static class DbContextAsyncExtensions
{
    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<TempTableOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new TempTableBuilder<T>(connection, transaction)
             .WithColumns(columnNamesSelector)
             .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
             .ConfigureTempTableOptions(configureOptions)
             .ExecuteAsync(data, cancellationToken);
    }

    public static Task<string> CreateTempTableAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<TempTableOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new TempTableBuilder<T>(connection, transaction)
             .WithColumns(columnNames)
             .WithMappingContext(dbContext.GetMappingContext(typeof(T)))
             .ConfigureTempTableOptions(configureOptions)
             .ExecuteAsync(data, cancellationToken);
    }
}
