using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class SqlConnectionExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Action<BulkDeleteOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkDeleteBuilder<T>(connectionContext)
            .WithId(idSelector)
             .ToTable(table)
                 .ConfigureBulkOptions(configureOptions)
      .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, string idColumn, Action<BulkDeleteOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkDeleteBuilder<T>(connectionContext)
 .WithId(idColumn)
        .ToTable(table)
  .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> idColumns, Action<BulkDeleteOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkDeleteBuilder<T>(connectionContext)
            .WithId(idColumns)
         .ToTable(table)
         .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, Action<BulkDeleteOptions> configureOptions = null)
    {
        return new BulkDeleteBuilder<T>(connectionContext)
          .WithId(idSelector)
         .ToTable(table)
       .ConfigureBulkOptions(configureOptions)
           .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, string idColumn, Action<BulkDeleteOptions> configureOptions = null)
    {
        return new BulkDeleteBuilder<T>(connectionContext)
        .WithId(idColumn)
                 .ToTable(table)
       .ConfigureBulkOptions(configureOptions)
                 .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, Action<BulkDeleteOptions> configureOptions = null)
    {
        return new BulkDeleteBuilder<T>(connectionContext)
    .WithId(idColumns)
          .ToTable(table)
  .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }
}
