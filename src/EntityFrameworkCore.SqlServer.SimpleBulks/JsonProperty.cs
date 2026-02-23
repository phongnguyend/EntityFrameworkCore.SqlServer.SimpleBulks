using Microsoft.EntityFrameworkCore.Storage.Json;
using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class JsonProperty
{
    public string JsonPropertyName { get; init; }

    public string ClrPropertyName { get; init; }

    public string FullJsonPropertyName { get; init; }

    public string FullClrPropertyName { get; init; }

    public bool IsShadowProperty { get; init; }

    public bool IsForeignKey { get; init; }

    public Type PropertyType { get; init; }

    public JsonValueReaderWriter? ReaderWriter { get; init; }

    public IReadOnlyList<JsonProperty>? NestedProperties { get; init; }

    public bool IsNestedComplexType => NestedProperties != null && NestedProperties.Count > 0;


    public static IReadOnlyDictionary<string, JsonProperty> FlattenJsonProperties(IReadOnlyList<JsonProperty>? jsonProperties)
    {
        if (jsonProperties == null || jsonProperties.Count == 0)
        {
            return new Dictionary<string, JsonProperty>();
        }

        var result = new Dictionary<string, JsonProperty>();
        FlattenJsonProperties(jsonProperties, result);
        return result;
    }

    private static void FlattenJsonProperties(IReadOnlyList<JsonProperty> jsonProperties, Dictionary<string, JsonProperty> result)
    {
        foreach (var jsonProperty in jsonProperties)
        {
            if (jsonProperty.IsNestedComplexType)
            {
                FlattenJsonProperties(jsonProperty.NestedProperties, result);
            }
            else
            {
                result[jsonProperty.FullClrPropertyName] = jsonProperty;
            }
        }
    }
}
