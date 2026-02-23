using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class ValueConverter
{
    public string PropertyName { get; init; }

    public Type ProviderClrType { get; init; }

    public Func<object?, object?> ConvertToProvider { get; init; }

    public Func<object?, object?> ConvertFromProvider { get; init; }

    public ValueConverter()
    {
    }
}

public class JsonValueConverter : ValueConverter
{
    public IReadOnlyList<JsonProperty>? JsonProperties { get; init; }

    public IReadOnlyDictionary<string, JsonProperty> FlattenedJsonProperties { get; init; }
}