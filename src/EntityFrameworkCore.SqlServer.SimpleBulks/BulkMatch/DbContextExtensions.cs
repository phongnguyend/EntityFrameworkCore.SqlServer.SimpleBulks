using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class DbContextExtensions
{
    public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new BulkMatchBuilder<T>(connection, transaction)
             .WithReturnedColumns(dbContext.GetAllPropertyNames(typeof(T)))
             .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
             .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
             .WithTable(table)
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
    {
        var table = dbContext.GetTableInfor(typeof(T));
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new BulkMatchBuilder<T>(connection, transaction)
             .WithReturnedColumns(returnedColumnsSelector)
             .WithDbColumnMappings(dbContext.GetColumnNames(typeof(T)))
             .WithDbColumnTypeMappings(dbContext.GetColumnTypes(typeof(T)))
             .WithTable(table)
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .Execute(machedValues);
    }
}
