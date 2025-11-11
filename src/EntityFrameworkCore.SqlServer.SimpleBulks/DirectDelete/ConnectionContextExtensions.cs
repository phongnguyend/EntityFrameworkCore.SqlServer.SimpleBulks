using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class ConnectionContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this ConnectionContext connectionContext, T data, SqlTableInfor<T> table = null, BulkDeleteOptions options = null)
    {
        var temp = table ?? TableMapper.Resolve<T>();

        return connectionContext.CreateBulkDeleteBuilder<T>()
        .WithId(temp.PrimaryKeys)
           .ToTable(temp)
              .WithBulkOptions(options)
     .SingleDelete(data);
    }
}