using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class DbContextExtensions
{
    public static BulkUpdateResult DirectUpdate<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));

        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNamesSelector)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleUpdate(data);
    }

    public static BulkUpdateResult DirectUpdate<T>(this DbContext dbContext, T data, IEnumerable<string> columnNames, BulkUpdateOptions options = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));

        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNames)
             .ToTable(table)
             .WithBulkOptions(options)
             .SingleUpdate(data);
    }
}
