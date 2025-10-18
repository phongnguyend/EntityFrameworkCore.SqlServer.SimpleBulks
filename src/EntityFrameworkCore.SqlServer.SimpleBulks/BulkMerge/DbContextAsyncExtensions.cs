using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public static class DbContextAsyncExtensions
{
    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> updateColumnNamesSelector, Expression<Func<T, object>> insertColumnNamesSelector, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var outputIdColumn = properties
            .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
            .Select(x => x.PropertyName)
            .FirstOrDefault();

        return new BulkMergeBuilder<T>(connection, transaction)
             .WithId(idSelector)
             .WithUpdateColumns(updateColumnNamesSelector)
             .WithInsertColumns(insertColumnNamesSelector)
             .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
             .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
             .WithOutputId(outputIdColumn)
             .ToTable(table)
             .ConfigureBulkOptions(configureOptions)
             .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, string idColumn, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var outputIdColumn = properties
            .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
            .Select(x => x.PropertyName)
            .FirstOrDefault();

        return new BulkMergeBuilder<T>(connection, transaction)
            .WithId(idColumn)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
            .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
            .WithOutputId(outputIdColumn)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }

    public static Task<BulkMergeResult> BulkMergeAsync<T>(this DbContext dbContext, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> updateColumnNames, IEnumerable<string> insertColumnNames, Action<BulkMergeOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var outputIdColumn = properties
            .Where(x => x.IsPrimaryKey && x.ValueGenerated == ValueGenerated.OnAdd)
            .Select(x => x.PropertyName)
            .FirstOrDefault();

        return new BulkMergeBuilder<T>(connection, transaction)
            .WithId(idColumns)
            .WithUpdateColumns(updateColumnNames)
            .WithInsertColumns(insertColumnNames)
            .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
            .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
            .WithOutputId(outputIdColumn)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .ExecuteAsync(data, cancellationToken);
    }
}
