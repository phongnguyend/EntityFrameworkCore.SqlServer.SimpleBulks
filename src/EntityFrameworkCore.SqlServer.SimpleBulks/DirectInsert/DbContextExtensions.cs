using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectInsert;

public static class DbContextExtensions
{
    public static void DirectInsert<T>(this DbContext dbContext, T data, BulkInsertOptions options = null)
    {
        var idColumn = dbContext.GetOutputId(typeof(T));

        dbContext.CreateBulkInsertBuilder<T>()
      .WithColumns(dbContext.GetInsertablePropertyNames(typeof(T)))
   .ToTable(dbContext.GetTableInfor(typeof(T)))
    .WithOutputId(idColumn?.PropertyName)
  .WithOutputIdMode(GetOutputIdMode(idColumn))
    .WithBulkOptions(options)
       .SingleInsert(data);
    }

    public static void DirectInsert<T>(this DbContext dbContext, T data, Expression<Func<T, object>> columnNamesSelector, BulkInsertOptions options = null)
    {
        var idColumn = dbContext.GetOutputId(typeof(T));

        dbContext.CreateBulkInsertBuilder<T>()
             .WithColumns(columnNamesSelector)
             .ToTable(dbContext.GetTableInfor(typeof(T)))
           .WithOutputId(idColumn?.PropertyName)
             .WithOutputIdMode(GetOutputIdMode(idColumn))
           .WithBulkOptions(options)
               .SingleInsert(data);
    }

    private static OutputIdMode GetOutputIdMode(ColumnInfor columnInfor)
    {
        if (columnInfor == null)
        {
            return OutputIdMode.ServerGenerated;
        }

        return columnInfor.PropertyType == typeof(Guid) && string.IsNullOrEmpty(columnInfor.DefaultValueSql) ? OutputIdMode.ClientGenerated : OutputIdMode.ServerGenerated;
    }
}
