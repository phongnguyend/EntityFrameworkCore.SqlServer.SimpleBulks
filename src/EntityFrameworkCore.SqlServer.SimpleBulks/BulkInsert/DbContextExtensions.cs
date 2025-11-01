using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class DbContextExtensions
{
    public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, BulkInsertOptions options = null)
    {
        var idColumn = dbContext.GetOutputId(typeof(T));

        new BulkInsertBuilder<T>(dbContext.GetConnectionContext())
            .WithColumns(dbContext.GetInsertablePropertyNames(typeof(T)))
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .WithOutputId(idColumn?.PropertyName)
            .WithOutputIdMode(GetOutputIdMode(idColumn))
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null)
    {
        var idColumn = dbContext.GetOutputId(typeof(T));

        new BulkInsertBuilder<T>(dbContext.GetConnectionContext())
            .WithColumns(columnNamesSelector)
            .ToTable(dbContext.GetTableInfor(typeof(T)))
            .WithOutputId(idColumn?.PropertyName)
            .WithOutputIdMode(GetOutputIdMode(idColumn))
            .WithBulkOptions(options)
            .Execute(data);
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
