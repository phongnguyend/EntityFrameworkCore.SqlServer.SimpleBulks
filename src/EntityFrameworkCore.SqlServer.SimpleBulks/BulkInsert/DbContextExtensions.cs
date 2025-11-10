using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public static class DbContextExtensions
{
    public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, BulkInsertOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        dbContext.CreateBulkInsertBuilder<T>()
            .WithColumns(table.InsertablePropertyNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .Execute(data);
    }

    public static void BulkInsert<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null)
    {
        dbContext.CreateBulkInsertBuilder<T>()
            .WithColumns(columnNamesSelector)
            .ToTable(dbContext.GetTableInfor<T>())
            .WithBulkOptions(options)
            .Execute(data);
    }
}
