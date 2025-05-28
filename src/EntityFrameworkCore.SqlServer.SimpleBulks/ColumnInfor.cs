using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class ColumnInfor
{
    public string PropertyName { get; set; }

    public Type PropertyType { get; set; }

    public string ColumnName { get; set; }

    public string ColumnType { get; set; }

    public ValueGenerated ValueGenerated { get; set; }

    public string DefaultValueSql { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsRowVersion { get; set; }
}
