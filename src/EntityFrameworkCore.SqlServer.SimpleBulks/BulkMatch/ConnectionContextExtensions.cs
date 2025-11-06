using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class ConnectionContextExtensions
{
    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, SqlTableInfor table = null, BulkMatchOptions options = null)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
                .WithReturnedColumns(returnedColumnsSelector)
        .WithTable(table ?? TableMapper.Resolve<T>())
           .WithMatchedColumns(matchedColumnsSelector)
                      .WithBulkOptions(options)
                      .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, SqlTableInfor table = null, BulkMatchOptions options = null)
    {
        return connectionContext.CreateBulkMatchBuilder<T>()
   .WithReturnedColumns(returnedColumns)
    .WithTable(table ?? TableMapper.Resolve<T>())
 .WithMatchedColumns(matchedColumns)
     .WithBulkOptions(options)
 .Execute(machedValues);
    }
}