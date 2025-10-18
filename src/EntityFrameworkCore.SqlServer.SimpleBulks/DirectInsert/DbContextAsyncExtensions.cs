using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class DbContextAsyncExtensions
{
    public static Task DirectInsertAsync<T>(this DbContext dbContext, T data, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var idColumn = dbContext.GetOutputId(typeof(T));

        return new BulkInsertBuilder<T>(connection, transaction)
              .WithColumns(dbContext.GetInsertablePropertyNames(typeof(T)))
              .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
              .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
              .ToTable(table)
              .WithOutputId(idColumn?.PropertyName)
              .WithOutputIdMode(GetOutputIdMode(idColumn))
              .ConfigureBulkOptions(configureOptions)
              .SingleInsertAsync(data, cancellationToken);
    }

    public static Task DirectInsertAsync<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var idColumn = dbContext.GetOutputId(typeof(T));

        return new BulkInsertBuilder<T>(connection, transaction)
             .WithColumns(columnNamesSelector)
             .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
             .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
             .ToTable(table)
             .WithOutputId(idColumn?.PropertyName)
             .WithOutputIdMode(GetOutputIdMode(idColumn))
             .ConfigureBulkOptions(configureOptions)
             .SingleInsertAsync(data, cancellationToken);
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
