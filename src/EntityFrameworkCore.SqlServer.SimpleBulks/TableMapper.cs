using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public static class TableMapper
{
    private static readonly ConcurrentDictionary<Type, TableInfor> _mappings = new ConcurrentDictionary<Type, TableInfor>();

    public static void Register<T>(TableInfor tableInfo)
    {
        Register(typeof(T), tableInfo);
    }

    public static TableInfor Resolve<T>()
    {
        return Resolve(typeof(T));
    }

    public static void Register(Type type, TableInfor tableInfo)
    {
        _mappings[type] = tableInfo;
    }

    public static TableInfor Resolve(Type type)
    {
        if (!_mappings.TryGetValue(type, out TableInfor tableInfo))
        {
            throw new Exception($"Type {type} has not been registered.");
        }

        return tableInfo;
    }
}
