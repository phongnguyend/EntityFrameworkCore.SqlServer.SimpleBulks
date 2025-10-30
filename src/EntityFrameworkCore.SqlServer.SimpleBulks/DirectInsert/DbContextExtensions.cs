using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class DbContextExtensions
{
    public static void DirectInsert<T>(this DbContext dbContext, T data, Action<BulkInsertOptions> configureOptions = null)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var idColumn = dbContext.GetOutputId(typeof(T));

        new BulkInsertBuilder<T>(connectionContext)
            .WithColumns(dbContext.GetInsertablePropertyNames(typeof(T)))
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .WithOutputId(idColumn?.PropertyName)
            .WithOutputIdMode(GetOutputIdMode(idColumn))
            .ConfigureBulkOptions(configureOptions)
            .SingleInsert(data);
    }

    public static void DirectInsert<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var idColumn = dbContext.GetOutputId(typeof(T));

        new BulkInsertBuilder<T>(connectionContext)
            .WithColumns(columnNamesSelector)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .WithOutputId(idColumn?.PropertyName)
            .WithOutputIdMode(GetOutputIdMode(idColumn))
            .ConfigureBulkOptions(configureOptions)
            .SingleInsert(data);
    }

    private static OutputIdMode GetOutputIdMode(ColumnInfor columnInfor)
    {
        if (columnInfor == null)
        {
            return OutputIdMode.ServerGenerated;
        }

        return columnInfor.PropertyType == typeof(Guid) && string.IsNullOrEmpty(columnInfor.DefaultValueSql) ? OutputIdMode.ClientGenerated : OutputIdMode.ServerGenerated;
    }
}
