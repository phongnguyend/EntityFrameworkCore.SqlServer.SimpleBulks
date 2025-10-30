using EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectUpdate;

public static class DbContextExtensions
{
    public static BulkUpdateResult DirectUpdate<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new BulkUpdateBuilder<T>(connectionContext)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithColumns(columnNamesSelector)
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .ConfigureBulkOptions(configureOptions)
             .SingleUpdate(data);
    }

    public static BulkUpdateResult DirectUpdate<T>(this DbContext dbContext, T data, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new BulkUpdateBuilder<T>(connectionContext)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithColumns(columnNames)
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .ConfigureBulkOptions(configureOptions)
             .SingleUpdate(data);
    }
}
