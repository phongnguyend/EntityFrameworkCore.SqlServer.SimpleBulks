using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class DbContextAsyncExtensions
{
    public static Task<BulkDeleteResult> BulkDeleteAsync<T>(this DbContext dbContext, IEnumerable<T> data, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        return dbContext.CreateBulkDeleteBuilder<T>()
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .WithBulkOptions(options)
             .ExecuteAsync(data, cancellationToken);
    }
}
