using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class ConnectionContextExtensions
{
    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor<T> table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .WithColumns(columnNamesSelector)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleUpdate(data);
    }

    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> columnNames, SqlTableInfor<T> table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .WithColumns(columnNames)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleUpdate(data);
    }

    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> keySelector, Expression<Func<T, object>> columnNamesSelector, SqlTableInfor<T> table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keySelector)
            .WithColumns(columnNamesSelector)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleUpdate(data);
    }

    public static BulkUpdateResult DirectUpdate<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> keys, IReadOnlyCollection<string> columnNames, SqlTableInfor<T> table = null, BulkUpdateOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkUpdateBuilder<T>()
            .WithId(keys)
            .WithColumns(columnNames)
            .ToTable(temp)
            .WithBulkOptions(options)
            .SingleUpdate(data);
    }
}