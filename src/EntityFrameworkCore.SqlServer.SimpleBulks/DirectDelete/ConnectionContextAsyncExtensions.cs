using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class ConnectionContextAsyncExtensions
{
    public static Task<BulkDeleteResult> DirectDeleteAsync<T>(this ConnectionContext connectionContext, T data, SqlTableInfor<T> table = null, BulkDeleteOptions options = null, CancellationToken cancellationToken = default)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
       .WithId(temp.PrimaryKeys)
         .ToTable(temp)
     .WithBulkOptions(options)
         .SingleDeleteAsync(data, cancellationToken);
    }
}