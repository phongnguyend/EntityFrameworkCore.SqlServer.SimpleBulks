using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, SqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(temp.PrimaryKeys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> keySelector, SqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keySelector)
            .ToTable(temp)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static BulkDeleteResult BulkDelete<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> keys, SqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
            .WithId(keys)
            .ToTable(temp)
            .WithBulkOptions(options)
            .Execute(data);
    }
}