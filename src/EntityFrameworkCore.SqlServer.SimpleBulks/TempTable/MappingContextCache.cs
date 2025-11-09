using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

internal static class MappingContextCache
{
    private readonly record struct CacheKey(Type DbContextType, Type EntityType);

    private static readonly ConcurrentDictionary<CacheKey, MappingContext> _dbContextMappingContextCache = [];
    private static readonly ConcurrentDictionary<Type, MappingContext> _typeMappingContextCache = [];

    public static MappingContext GetMappingContext(this DbContext dbContext, Type type)
    {
        var cacheKey = new CacheKey(dbContext.GetType(), type);

        return _dbContextMappingContextCache.GetOrAdd(cacheKey, (key) =>
        {
            var isEntityType = dbContext.IsEntityType(type);

            if (!isEntityType)
            {
                return MappingContext.Default;
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

    public static MappingContext GetMappingContext(this Type type)
    {
        return _typeMappingContextCache.GetOrAdd(type, (type) =>
        {
            if (!TableMapper.TryResolve(type, out var table))
            {
                return MappingContext.Default;
            }

            var mappingContext = new MappingContext
            {
                ColumnNameMappings = table.ColumnNameMappings,
                ColumnTypeMappings = table.ColumnTypeMappings,
                ValueConverters = table.ValueConverters
            };

            return mappingContext;
        });
    }
}
