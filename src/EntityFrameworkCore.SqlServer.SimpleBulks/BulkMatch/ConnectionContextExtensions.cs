using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class ConnectionContextExtensions
{
    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, BulkMatchOptions options = null)
    {
        var table = TableMapper.Resolve<T>(options);

        return connectionContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(table.PropertyNames)
            .WithTable(table)
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
             .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, BulkMatchOptions options = null)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
                .WithReturnedColumns(returnedColumnsSelector)
        .WithTable(TableMapper.Resolve<T>(options))
           .WithMatchedColumns(matchedColumnsSelector)
                      .WithBulkOptions(options)
                      .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, IReadOnlyCollection<string> matchedColumns, IReadOnlyCollection<string> returnedColumns, BulkMatchOptions options = null)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
   .WithReturnedColumns(returnedColumns)
    .WithTable(TableMapper.Resolve<T>(options))
 .WithMatchedColumns(matchedColumns)
     .WithBulkOptions(options)
 .Execute(machedValues);
    }
}