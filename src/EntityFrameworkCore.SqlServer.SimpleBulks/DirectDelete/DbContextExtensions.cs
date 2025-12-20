using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, BulkDeleteOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(table.PrimaryKeys)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, Expression<Func<T, object>> keySelector, BulkDeleteOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(keySelector)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleDelete(data);
    }

    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, IReadOnlyCollection<string> keys, BulkDeleteOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(keys)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleDelete(data);
    }
}
