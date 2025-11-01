using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this DbContext dbContext, T data, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkDeleteBuilder<T>()
          .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .WithBulkOptions(options)
       .SingleDeleteAsync(data, cancellationToken);
    }
}
