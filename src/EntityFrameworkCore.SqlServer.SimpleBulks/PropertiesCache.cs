using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Reflection;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class PropertiesCache
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> _cache = new();

    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties(Type type)
    {
        return _cache.GetOrAdd(type, _ =>
        {
            var properties = type.GetProperties();
            return properties.ToFrozenDictionary(p => p.Name, p => p);
        });
    }

    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties<T>()
    {
        return GetProperties(typeof(T));
    }
}

public class PropertiesCache<T>
{
    public static IReadOnlyDictionary<string, PropertyInfo> GetProperties()
    {
        return PropertiesCache.GetProperties(typeof(T));
    }

    public static PropertyInfo GetProperty(string name)
    {
        return GetProperties().TryGetValue(name, out var property) ? property : null;
    }
}
