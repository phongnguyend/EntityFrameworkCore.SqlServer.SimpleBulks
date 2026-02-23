using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, BulkDeleteOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(table.PrimaryKeys)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> keySelector, BulkDeleteOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keySelector)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> keys, BulkDeleteOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keys)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleDelete(data);
    }
}