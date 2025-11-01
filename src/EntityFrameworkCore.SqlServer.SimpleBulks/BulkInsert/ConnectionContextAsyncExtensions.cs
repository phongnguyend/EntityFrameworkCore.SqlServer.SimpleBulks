using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class ConnectionContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
            .WithColumns(columnNamesSelector)
          .ToTable(TableMapper.Resolve(typeof(T)))
     .WithBulkOptions(options)
 .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
              .WithColumns(columnNamesSelector)
                 .ToTable(TableMapper.Resolve(typeof(T)))
                  .WithOutputId(idSelector)
                  .WithBulkOptions(options)
                .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
           .WithColumns(columnNames)
        .ToTable(TableMapper.Resolve(typeof(T)))
                  .WithBulkOptions(options)
          .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
          .WithColumns(columnNames)
            .ToTable(TableMapper.Resolve(typeof(T)))
         .WithOutputId(idColumnName)
               .WithBulkOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
      .WithColumns(columnNamesSelector)
    .ToTable(table)
             .WithBulkOptions(options)
       .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
        .WithColumns(columnNamesSelector)
        .ToTable(table)
       .WithOutputId(idSelector)
               .WithBulkOptions(options)
        .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
            .WithColumns(columnNames)
             .ToTable(table)
         .WithBulkOptions(options)
           .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, string idColumnName, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
     .WithColumns(columnNames)
     .ToTable(table)
     .WithOutputId(idColumnName)
         .WithBulkOptions(options)
      .ExecuteAsync(data, cancellationToken);
    }
}