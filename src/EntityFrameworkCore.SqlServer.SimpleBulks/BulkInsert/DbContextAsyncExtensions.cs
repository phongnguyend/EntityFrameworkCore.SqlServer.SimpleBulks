using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class DbContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var idColumn = dbContext.GetOutputId(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext)
              .WithColumns(dbContext.GetInsertablePropertyNames(typeof(T)))
              .ToTable(dbContext.GetTableInfor(typeof(T)))
              .WithOutputId(idColumn?.PropertyName)
              .WithOutputIdMode(GetOutputIdMode(idColumn))
              .WithBulkOptions(options)
              .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null, CancellationToken cancellationToken = default)
    {
        var connectionContext = dbContext.GetConnectionContext();
        var idColumn = dbContext.GetOutputId(typeof(T));

        return new BulkInsertBuilder<T>(connectionContext)
              .WithColumns(columnNamesSelector)
              .ToTable(dbContext.GetTableInfor(typeof(T)))
              .WithOutputId(idColumn?.PropertyName)
              .WithOutputIdMode(GetOutputIdMode(idColumn))
              .WithBulkOptions(options)
              .ExecuteAsync(data, cancellationToken);
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
