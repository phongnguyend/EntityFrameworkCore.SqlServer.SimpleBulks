using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class ValueConverter
{
    public Type ProviderClrType { get; init; }

    public Func<object?, object?> ConvertToProvider { get; init; }

    public Func<object?, object?> ConvertFromProvider { get; init; }

    public ValueConverter()
    {
    }

    public ValueConverter(Type providerClrType, Func<object?, object?> convertToProvider, Func<object?, object?> convertFromProvider)
    {
        ProviderClrType = providerClrType;
        ConvertToProvider = convertToProvider;
        ConvertFromProvider = convertFromProvider;
    }
}
