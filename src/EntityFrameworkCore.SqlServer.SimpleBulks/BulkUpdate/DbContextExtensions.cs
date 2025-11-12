using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public static class DbContextExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IReadOnlyCollection<T> data, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();
        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNamesSelector)
             .ToTable(table)
             .WithBulkOptions(options)
             .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IReadOnlyCollection<T> data, IReadOnlyCollection<string> columnNames, BulkUpdateOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(table.PrimaryKeys)
             .WithColumns(columnNames)
             .ToTable(table)
             .WithBulkOptions(options)
             .Execute(data);
    }
}
