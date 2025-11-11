using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public static class TableMapper
{
    private static readonly ConcurrentDictionary<Type, object> _mappings = new ConcurrentDictionary<Type, object>();

    public static void Register<T>(TableInfor<T> tableInfo)
    {
        _mappings[typeof(T)] = tableInfo;
    }

    public static TableInfor<T> Resolve<T>()
    {
        if (!_mappings.TryGetValue(typeof(T), out var tableInfo))
        {
            throw new Exception($"Type {typeof(T)} has not been registered.");
        }

        return (TableInfor<T>)tableInfo;
    }

    public static bool TryResolve<T>(out TableInfor<T> tableInfo)
    {
        if (_mappings.TryGetValue(typeof(T), out var info))
        {
            tableInfo = (TableInfor<T>)info;
            return true;
        }

        tableInfo = null;
        return false;
    }
}
