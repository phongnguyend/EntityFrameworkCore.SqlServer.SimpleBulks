using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public static class DbContextExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new BulkUpdateBuilder<T>(connection, transaction)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .WithColumns(columnNamesSelector)
             .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
             .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
             .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
             .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new BulkUpdateBuilder<T>(connection, transaction)
            .WithId(dbContext.GetPrimaryKeys(typeof(T)))
            .WithColumns(columnNames)
            .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
            .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }
}
