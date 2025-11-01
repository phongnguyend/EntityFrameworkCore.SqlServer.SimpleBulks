using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public static class DbContextExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkUpdateOptions options = null)
    {
        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithColumns(columnNamesSelector)
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .WithBulkOptions(options)
             .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, BulkUpdateOptions options = null)
    {
        return dbContext.CreateBulkUpdateBuilder<T>()
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithColumns(columnNames)
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .WithBulkOptions(options)
             .Execute(data);
    }
}
