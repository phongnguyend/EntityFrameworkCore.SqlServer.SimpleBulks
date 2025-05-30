using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetOrSet<TKey, TValue>(this Dictionary<TKey, TValue> cache, object lockObject, TKey key, Func<TValue> valueFactory)
    {
        if (cache.TryGetValue(key, out TValue value1))
        {
            return value1;
        }

        lock (lockObject)
        {
            if (cache.TryGetValue(key, out TValue value2))
            {
                return value2;
            }

            var value = valueFactory();

            cache[key] = value;

            return value;
        }
    }
}
