using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class ConnectionContextExtensions
{
    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, SqlTableInfor<T> table = null, BulkMatchOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkMatchBuilder<T>()
            .WithReturnedColumns(temp.PropertyNames)
            .WithTable(temp)
            .WithMatchedColumns(matchedColumnsSelector)
            .WithBulkOptions(options)
             .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, SqlTableInfor<T> table = null, BulkMatchOptions options = null)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
                .WithReturnedColumns(returnedColumnsSelector)
        .WithTable(table ?? TableMapper.Resolve<T>())
           .WithMatchedColumns(matchedColumnsSelector)
                      .WithBulkOptions(options)
                      .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IReadOnlyCollection<T> machedValues, IReadOnlyCollection<string> matchedColumns, IReadOnlyCollection<string> returnedColumns, SqlTableInfor<T> table = null, BulkMatchOptions options = null)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
   .WithReturnedColumns(returnedColumns)
    .WithTable(table ?? TableMapper.Resolve<T>())
 .WithMatchedColumns(matchedColumns)
     .WithBulkOptions(options)
 .Execute(machedValues);
    }
}