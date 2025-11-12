using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class DbContextExtensions
{
    public static List<T> BulkMatch<T>(this DbContext dbContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, BulkMatchOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        return dbContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(table.PropertyNames)
            .WithTable(table)
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
            .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this DbContext dbContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, BulkMatchOptions options = null)
    {
        return dbContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(returnedColumnsSelector)
            .WithTable(dbContext.GetTableInfor<T>())
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
            .Execute(machedValues);
    }
}
