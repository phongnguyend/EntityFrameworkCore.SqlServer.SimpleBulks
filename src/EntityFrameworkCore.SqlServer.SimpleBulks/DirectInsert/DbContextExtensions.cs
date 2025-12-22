using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class DbContextExtensions
{
    public static void DirectInsert<T>(this DbContext dbContext, T data, BulkInsertOptions options = null)
    {
        var table = dbContext.GetTableInfor<T>();

        dbContext.CreateBulkInsertBuilder<T>()
            .WithColumns(table.InsertablePropertyNames)
            .ToTable(table)
            .WithBulkOptions(options)
            .SingleInsert(data);
    }

    public static void DirectInsert<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null)
    {
        dbContext.CreateBulkInsertBuilder<T>()
            .WithColumns(columnNamesSelector)
            .ToTable(dbContext.GetTableInfor<T>())
            .WithBulkOptions(options)
            .SingleInsert(data);
    }

    public static void DirectInsert<T>(this DbContext dbContext, T data, IReadOnlyCollection<string> columnNames, BulkInsertOptions options = null)
    {
        dbContext.CreateBulkInsertBuilder<T>()
            .WithColumns(columnNames)
            .ToTable(dbContext.GetTableInfor<T>())
            .WithBulkOptions(options)
            .SingleInsert(data);
    }
}
