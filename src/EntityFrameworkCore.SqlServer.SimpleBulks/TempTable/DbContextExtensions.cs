using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public static class DbContextExtensions
{
    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, Expression<Func<T, object>> columnNamesSelector, Action<TempTableOptions> configureOptions = null)
    {
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        var isEntityType = dbContext.IsEntityType(typeof(T));

        IReadOnlyDictionary<string, string> columnNameMappings = null;
        IReadOnlyDictionary<string, string> columnTypeMappings = null;

        if (isEntityType)
        {
            var properties = dbContext.GetProperties(typeof(T));
            columnNameMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);
            columnTypeMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnType);
        }

        return new TempTableBuilder<T>(connection, transaction)
             .WithData(data)
             .WithColumns(columnNamesSelector)
             .WithDbColumnMappings(columnNameMappings)
             .WithDbColumnTypeMappings(columnTypeMappings)
             .ConfigureTempTableOptions(configureOptions)
             .Execute();
    }

    public static string CreateTempTable<T>(this DbContext dbContext, IEnumerable<T> data, Action<TempTableOptions> configureOptions = null)
    {
        var connection = dbContext.GetSqlConnection();
        var transaction = dbContext.GetCurrentSqlTransaction();

        var isEntityType = dbContext.IsEntityType(typeof(T));

        IReadOnlyDictionary<string, string> columnNameMappings = null;
        IReadOnlyDictionary<string, string> columnTypeMappings = null;
        IEnumerable<string> columnNames = typeof(T).GetDbColumnNames();

        if (isEntityType)
        {
            var properties = dbContext.GetProperties(typeof(T));
            columnNameMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnName);
            columnTypeMappings = properties.ToDictionary(x => x.PropertyName, x => x.ColumnType);
        }

        return new TempTableBuilder<T>(connection, transaction)
             .WithData(data)
             .WithColumns(columnNames)
             .WithDbColumnMappings(columnNameMappings)
             .WithDbColumnTypeMappings(columnTypeMappings)
             .ConfigureTempTableOptions(configureOptions)
             .Execute();
    }
}
