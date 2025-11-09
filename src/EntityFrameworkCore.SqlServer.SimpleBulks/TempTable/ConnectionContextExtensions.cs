using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public static class ConnectionContextExtensions
{
    public static string CreateTempTable<T>(this ConnectionContext connectionContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, TempTableOptions options = null)
    {
        return connectionContext.CreateTempTableBuilder<T>()
            .WithColumns(columnNamesSelector)
            .WithMappingContext(typeof(T).GetMappingContext())
            .WithTempTableOptions(options)
            .Execute(data);
    }

    public static string CreateTempTable<T>(this ConnectionContext connectionContext, IEnumerable<T> data, IEnumerable<string> columnNames, TempTableOptions options = null)
    {
        return connectionContext.CreateTempTableBuilder<T>()
            .WithColumns(columnNames)
            .WithMappingContext(typeof(T).GetMappingContext())
            .WithTempTableOptions(options)
            .Execute(data);
    }
}
