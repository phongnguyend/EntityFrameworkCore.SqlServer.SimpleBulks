using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public static class TableMapper
{
    private static readonly object _lock = new object();
    private static readonly Dictionary<Type, TableInfor> _mappings = new Dictionary<Type, TableInfor>();

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
        lock (_lock)
        {
            _mappings[type] = tableInfo;
        }
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
