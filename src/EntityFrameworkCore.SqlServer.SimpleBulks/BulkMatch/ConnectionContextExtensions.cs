using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public static class ConnectionContextExtensions
{
    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, BulkMatchOptions options = null)
    {
        return new BulkMatchBuilder<T>(connectionContext)
               .WithReturnedColumns(returnedColumnsSelector)
             .WithTable(TableMapper.Resolve(typeof(T)))
          .WithMatchedColumns(matchedColumnsSelector)
               .WithBulkOptions(options)
               .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, string matchedColumn, IEnumerable<string> returnedColumns, BulkMatchOptions options = null)
    {
        return new BulkMatchBuilder<T>(connectionContext)
             .WithReturnedColumns(returnedColumns)
              .WithTable(TableMapper.Resolve(typeof(T)))
             .WithMatchedColumn(matchedColumn)
        .WithBulkOptions(options)
             .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, BulkMatchOptions options = null)
    {
        return new BulkMatchBuilder<T>(connectionContext)
              .WithReturnedColumns(returnedColumns)
              .WithTable(TableMapper.Resolve(typeof(T)))
            .WithMatchedColumns(matchedColumns)
              .WithBulkOptions(options)
           .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, TableInfor table, Expression<Func<T, object>> matchedColumnsSelector, Expression<Func<T, object>> returnedColumnsSelector, BulkMatchOptions options = null)
    {
        return new BulkMatchBuilder<T>(connectionContext)
              .WithReturnedColumns(returnedColumnsSelector)
         .WithTable(table)
              .WithMatchedColumns(matchedColumnsSelector)
              .WithBulkOptions(options)
             .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, TableInfor table, string matchedColumns, IEnumerable<string> returnedColumns, BulkMatchOptions options = null)
    {
        return new BulkMatchBuilder<T>(connectionContext)
   .WithReturnedColumns(returnedColumns)
         .WithTable(table)
     .WithMatchedColumn(matchedColumns)
         .WithBulkOptions(options)
            .Execute(machedValues);
    }

    public static List<T> BulkMatch<T>(this ConnectionContext connectionContext, IEnumerable<T> machedValues, TableInfor table, IEnumerable<string> matchedColumns, IEnumerable<string> returnedColumns, BulkMatchOptions options = null)
    {
        return new BulkMatchBuilder<T>(connectionContext)
        .WithReturnedColumns(returnedColumns)
     .WithTable(table)
    .WithMatchedColumns(matchedColumns)
        .WithBulkOptions(options)
       .Execute(machedValues);
    }
}