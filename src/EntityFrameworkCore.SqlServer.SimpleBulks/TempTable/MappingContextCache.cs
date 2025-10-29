using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

internal static class MappingContextCache
{
    private readonly record struct CacheKey(Type DbContextType, Type EntityType);

    private static readonly ConcurrentDictionary<CacheKey, MappingContext> _mappingContextCache = [];

    public static MappingContext GetMappingContext(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);

        return _mappingContextCache.GetOrAdd(cacheKey, (key) =>
        {
            var isEntityType = dbContext.IsEntityType(type);

            if (!isEntityType)
            {
                return new MappingContext();
            }

            var mappingContext = new MappingContext
            {
                ColumnNameMappings = dbContext.GetColumnNames(key.EntityType),
                ColumnTypeMappings = dbContext.GetColumnTypes(key.EntityType),
                ValueConverters = dbContext.GetValueConverters(key.EntityType)
            };
            return mappingContext;
        });
    }
}
