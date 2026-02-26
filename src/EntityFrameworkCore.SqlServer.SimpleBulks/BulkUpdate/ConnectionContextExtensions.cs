using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public static class ConnectionContextExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(table.PrimaryKeys)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, BulkUpdateOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(table.PrimaryKeys)
            .WithColumns(columnNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> keySelector, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keySelector)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> keys, IReadOnlyCollection<string> columnNames, BulkUpdateOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keys)
            .WithColumns(columnNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }
}