using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class ColumnInfor
{
    public string PropertyName { get; init; }

    public Type PropertyType { get; init; }

    public string ColumnName { get; init; }

    public string ColumnType { get; init; }

    public ValueGenerated ValueGenerated { get; init; }

    public string DefaultValueSql { get; init; }

    public bool IsPrimaryKey { get; init; }

    public bool IsRowVersion { get; init; }

    public ValueConverter? ValueConverter { get; set; }
}
