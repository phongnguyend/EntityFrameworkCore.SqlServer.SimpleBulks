using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class ConnectionContextAsyncExtensions
{
    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMatchBuilder<T>(connectionContext)
 .WithReturnedColumns(returnedColumnsSelector)
      .WithTable(table)
           .WithMatchedColumns(matchedColumnsSelector)
       .ConfigureBulkOptions(configureOptions)
     .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, string matchedColumn, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMatchBuilder<T>(connectionContext)
        .WithReturnedColumns(returnedColumns)
       .WithTable(table)
          .WithMatchedColumn(matchedColumn)
              .ConfigureBulkOptions(configureOptions)
             .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkMatchBuilder<T>(connectionContext)
   .WithReturnedColumns(returnedColumns)
         .WithTable(table)
    .WithMatchedColumns(matchedColumns)
       .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, TableInfor table, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMatchBuilder<T>(connectionContext)
             .WithReturnedColumns(returnedColumnsSelector)
             .WithTable(table)
        .WithMatchedColumns(matchedColumnsSelector)
       .ConfigureBulkOptions(configureOptions)
           .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, TableInfor table, string matchedColumns, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMatchBuilder<T>(connectionContext)
    .WithReturnedColumns(returnedColumns)
            .WithTable(table)
  .WithMatchedColumn(matchedColumns)
      .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, TableInfor table, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkMatchBuilder<T>(connectionContext)
  .WithReturnedColumns(returnedColumns)
            .WithTable(table)
            .WithMatchedColumns(matchedColumns)
    .ConfigureBulkOptions(configureOptions)
          .ExecuteAsync(machedValues, cancellationToken);
    }
}