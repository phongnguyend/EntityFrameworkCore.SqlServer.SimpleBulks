using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, BulkDeleteOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(table.PrimaryKeys)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> keySelector, BulkDeleteOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keySelector)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> keys, BulkDeleteOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keys)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }
}