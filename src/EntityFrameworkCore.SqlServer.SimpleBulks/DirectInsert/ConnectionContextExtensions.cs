using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class ConnectionContextExtensions
{
    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, BulkInsertOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        connectionContext.CreateBulkInsertBuilder<T>()
            .WithColumns(table.InsertablePropertyNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleInsert(data);
    }

    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
            .WithColumns(columnNamesSelector)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .SingleInsert(data);
    }

    public static void DirectInsert<T>(this ConnectionContext connectionContext, T data, IReadOnlyCollection<string> columnNames, BulkInsertOptions options = null)
    {
        connectionContext.CreateBulkInsertBuilder<T>()
            .WithColumns(columnNames)
            .ToTable(TableMapper.Resolve<T>(options))
            .WithBulkOptions(options)
            .SingleInsert(data);
    }
}