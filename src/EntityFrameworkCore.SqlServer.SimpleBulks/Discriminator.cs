using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public record Discriminator
{
    public string PropertyName { get; init; }

    public Type PropertyType { get; init; }

    public object PropertyValue { get; init; }

    public string ColumnName { get; init; }

    public string ColumnType { get; init; }
}
