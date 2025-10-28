using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public static class DbContextExtensions
{
    public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkDeleteOptions> configureOptions = null)
    {
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        return new BulkDeleteBuilder<T>(connection, transaction)
             .WithId(dbContext.GetPrimaryKeys(typeof(T)))
             .ToTable(dbContext.GetTableInfor(typeof(T)))
             .ConfigureBulkOptions(configureOptions)
             .Execute(data);
    }
}
