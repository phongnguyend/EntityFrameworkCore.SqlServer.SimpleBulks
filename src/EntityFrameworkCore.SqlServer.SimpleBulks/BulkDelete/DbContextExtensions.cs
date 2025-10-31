using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null)
    {
        var connectionContext = dbContext.GetConnectionContext();

        return new BulkDeleteBuilder<T>(connectionContext)
       .WithId(dbContext.GetPrimaryKeys(typeof(T)))
        .ToTable(dbContext.GetTableInfor(typeof(T)))
         .WithBulkOptions(options)
        .Execute(data);
    }
}
