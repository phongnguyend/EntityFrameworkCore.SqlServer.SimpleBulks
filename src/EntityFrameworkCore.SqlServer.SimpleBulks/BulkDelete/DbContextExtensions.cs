using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null)
    {
        return dbContext.CreateBulkDeleteBuilder<T>()
         .WithId(dbContext.GetPrimaryKeys(typeof(T)))
      .ToTable(dbContext.GetTableInfor(typeof(T)))
    .WithBulkOptions(options)
          .Execute(data);
    }
}
