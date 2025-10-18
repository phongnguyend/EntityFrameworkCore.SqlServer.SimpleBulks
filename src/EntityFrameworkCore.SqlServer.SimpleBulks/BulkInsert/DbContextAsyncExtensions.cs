using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class DbContextAsyncExtensions
{
    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var columns = properties
            .Where(x => x.ValueGenerated == ValueGenerated.Never)
            .Select(x => x.PropertyName);
        var idColumn = properties
            .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
            .FirstOrDefault();

        return new BulkInsertBuilder<T>(connection, transaction)
              .WithColumns(columns)
              .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
              .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
              .ToTable(table)
              .WithOutputId(idColumn?.PropertyName)
              .WithOutputIdMode(GetOutputIdMode(idColumn))
              .ConfigureBulkOptions(configureOptions)
              .ExecuteAsync(data, cancellationToken);
    }

    public static Task BulkInsertAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<BulkInsertOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var idColumn = properties
            .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
            .FirstOrDefault();

        return new BulkInsertBuilder<T>(connection, transaction)
              .WithColumns(columnNamesSelector)
              .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
              .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
              .ToTable(table)
              .WithOutputId(idColumn?.PropertyName)
              .WithOutputIdMode(GetOutputIdMode(idColumn))
              .ConfigureBulkOptions(configureOptions)
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
