﻿using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class DbContextAsyncExtensions
{
    public static Task<List<T>> BulkMatchAsync<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));
        var columns = properties.Select(x => x.PropertyName);

        return new BulkMatchBuilder<T>(connection, transaction)
             .WithReturnedColumns(columns)
             .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
             .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
             .WithTable(table)
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .ExecuteAsync(machedValues, cancellationToken);
    }

    public static Task<List<T>> BulkMatchAsync<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null, CancellationToken cancellationToken = default)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();
        var properties = dbContext.GetProperties(typeof(T));

        return new BulkMatchBuilder<T>(connection, transaction)
             .WithReturnedColumns(returnedColumnsSelector)
             .WithDbColumnMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnName))
             .WithDbColumnTypeMappings(properties.ToDictionary(x => x.PropertyName, x => x.ColumnType))
             .WithTable(table)
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .ExecuteAsync(machedValues, cancellationToken);
    }
}
