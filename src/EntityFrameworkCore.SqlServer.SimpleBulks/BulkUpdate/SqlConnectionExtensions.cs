﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public static class SqlConnectionExtensions
{
    public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkUpdateBuilder<T>(connection)
            .WithId(idSelector)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, string idColumn, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkUpdateBuilder<T>(connection)
            .WithId(idColumn)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, IEnumerable<string> idColumns, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
    {
        var table = TableMapper.Resolve(typeof(T));

        return new BulkUpdateBuilder<T>(connection)
            .WithId(idColumns)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, TableInfor table, Expression<Func<T, object>> idSelector, Expression<Func<T, object>> columnNamesSelector, Action<BulkUpdateOptions> configureOptions = null)
    {
        return new BulkUpdateBuilder<T>(connection)
            .WithId(idSelector)
            .WithColumns(columnNamesSelector)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, TableInfor table, string idColumn, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
    {
        return new BulkUpdateBuilder<T>(connection)
            .WithId(idColumn)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }

    public static BulkUpdateResult BulkUpdate<T>(this SqlConnection connection, IEnumerable<T> data, TableInfor table, IEnumerable<string> idColumns, IEnumerable<string> columnNames, Action<BulkUpdateOptions> configureOptions = null)
    {
        return new BulkUpdateBuilder<T>(connection)
            .WithId(idColumns)
            .WithColumns(columnNames)
            .ToTable(table)
            .ConfigureBulkOptions(configureOptions)
            .Execute(data);
    }
}
