using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable
{
    public static class DbContextExtensions
    {
        public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            return new TempTableBuilder<T>(connection, transaction)
                 .WithData(data)
                 .WithColumns(columnNamesSelector)
                 .Execute();
        }

        public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data)
        {
            var connection = dbContext.GetSqlConnection();
            var transaction = dbContext.GetCurrentSqlTransaction();

            return new TempTableBuilder<T>(connection, transaction)
                 .WithData(data)
                 .WithColumns(typeof(T).GetDbColumnNames())
                 .Execute();
        }
    }
}
