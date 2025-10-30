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
        var connectionContext = dbContext.GetConnectionContext();

        return new BulkMatchBuilder<T>(connectionContext)
             .WithReturnedColumns(dbContext.GetAllPropertyNames(typeof(T)))
             .WithTable(dbContext.GetTableInfor(typeof(T)))
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, Action<BulkMatchOptions> configureOptions = null)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new BulkMatchBuilder<T>(connectionContext)
             .WithReturnedColumns(returnedColumnsSelector)
             .WithTable(dbContext.GetTableInfor(typeof(T)))
             .WithMatchedColumns(matchedColumnsSelector)
             .ConfigureBulkOptions(configureOptions)
             .Execute(machedValues);
    }
}
