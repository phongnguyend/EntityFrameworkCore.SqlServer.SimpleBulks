using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class DbContextExtensions
{
    public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, BulkMatchOptions options = null)
    {
        return dbContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(dbContext.GetAllPropertyNames(typeof(T)))
            .WithTable(dbContext.GetTableInfor(typeof(T)))
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
            .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this DbContext dbContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, BulkMatchOptions options = null)
    {
        return dbContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(returnedColumnsSelector)
            .WithTable(dbContext.GetTableInfor(typeof(T)))
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
            .Execute(machedValues);
    }
}
