using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions
{
    public static class DbContextExtensions
    {
        public static string GetTableName(this DbContext dbContext, Type type)
        {
            var entityType = dbContext.Model.FindEntityType(type);
            return entityType.GetSchemaQualifiedTableName();
        }

        public static SqlConnection GetSqlConnection(this DbContext dbContext)
        {
            return dbContext.Database.GetDbConnection().AsSqlConnection();
        }
    }
}
