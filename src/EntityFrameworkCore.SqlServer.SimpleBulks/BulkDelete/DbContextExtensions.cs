using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete
{
    public static class DbContextExtensions
    {
        public static BulkDeleteResult BulkDelete<T>(this DbContext dbContext, IEnumerable<T> data, Action<BulkDeleteOptions> configureOptions = null)
        {
            string tableName = dbContext.GetTableName(typeof(T));
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();
            var properties = dbContext.GetProperties(typeof(T));
            var primaryKeys = properties
                .Where(x => x.IsPrimaryKey)
                .Select(x => x.PropertyName);
            var dbColumnMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);

            return new BulkDeleteBuilder<T>(connection, transaction)
                 .WithData(data)
                 .WithId(primaryKeys)
                 .WithDbColumnMappings(dbColumnMappings)
                 .ToTable(tableName)
                 .ConfigureBulkOptions(configureOptions)
                 .Execute();
        }
    }
}
