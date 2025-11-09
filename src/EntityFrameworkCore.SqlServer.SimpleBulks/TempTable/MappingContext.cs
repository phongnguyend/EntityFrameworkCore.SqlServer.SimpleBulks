using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public class MappingContext
{
    public static readonly MappingContext Default = new MappingContext();

    public IReadOnlyDictionary<string, string> ColumnNameMappings { get; init; }

    public IReadOnlyDictionary<string, string> ColumnTypeMappings { get; init; }

    public IReadOnlyDictionary<string, ValueConverter> ValueConverters { get; init; }
}
