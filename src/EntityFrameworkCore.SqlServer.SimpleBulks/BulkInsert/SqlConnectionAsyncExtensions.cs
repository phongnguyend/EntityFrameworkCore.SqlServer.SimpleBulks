using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class SqlConnectionAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext)
             .WithColumns(columnNamesSelector)
 .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
  .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext)
    .WithColumns(columnNamesSelector)
            .ToTable(table)
      .WithOutputId(idSelector)
               .ConfigureBulkOptions(configureOptions)
      .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext)
       .WithColumns(columnNames)
     .ToTable(table)
                .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext)
   .WithColumns(columnNames)
            .ToTable(table)
       .WithOutputId(idColumnName)
       .ConfigureBulkOptions(configureOptions)
.ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
     .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> columnNamesSelector, Expression<Func<T, object>> idSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
          .WithColumns(columnNamesSelector)
                 .ToTable(table)
                 .WithOutputId(idSelector)
                 .ConfigureBulkOptions(configureOptions)
             .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
                .WithColumns(columnNames)
           .ToTable(table)
        .ConfigureBulkOptions(configureOptions)
                   .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> columnNames, string idColumnName, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        return new BulkInsertBuilder<T>(connectionContext)
           .WithColumns(columnNames)
     .ToTable(table)
               .WithOutputId(idColumnName)
         .ConfigureBulkOptions(configureOptions)
           .ExecuteAsync(data, cancellationToken);
    }
}
