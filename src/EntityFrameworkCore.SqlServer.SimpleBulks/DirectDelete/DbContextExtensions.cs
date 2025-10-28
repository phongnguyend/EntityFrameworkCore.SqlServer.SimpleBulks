using EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;
using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DirectDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult DirectDelete<T>(this DbContext dbContext, T data, Action<BulkDeleteOptions> configureOptions = null)
    {
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new BulkDeleteBuilder<T>(connection, transaction)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .ConfigureBulkOptions(configureOptions)
             .SingleDelete(data);
    }
}
