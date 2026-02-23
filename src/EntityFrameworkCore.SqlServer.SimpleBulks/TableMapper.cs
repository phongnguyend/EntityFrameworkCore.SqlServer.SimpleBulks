using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public static class TableMapper
{
    private static readonly ConcurrentDictionary<Type, object> _mappings = new ConcurrentDictionary<Type, object>();
    private static readonly ConcurrentDictionary<(Type, string), object> _namedMappings = new ConcurrentDictionary<(Type, string), object>();

    public static void Register<T>(TableInfor<T> tableInfo)
    {
        _mappings[typeof(T)] = tableInfo;
    }

    public static void Register<T>(string name, TableInfor<T> tableInfo)
    {
        _namedMappings[(typeof(T), name)] = tableInfo;
    }

    public static void Configure<T>(Action<SqlTableInforBuilder<T>> config)
    {
        var builder = new SqlTableInforBuilder<T>();
        config(builder);
        Register(builder.Build());
    }

    public static void Configure<T>(string name, Action<SqlTableInforBuilder<T>> config)
    {
        var builder = new SqlTableInforBuilder<T>();
        config(builder);
        Register(name, builder.Build());
    }

    public static TableInfor<T> Resolve<T>()
    {
        if (!_mappings.TryGetValue(typeof(T), out var tableInfo))
        {
            throw new Exception($"Type {typeof(T)} has not been registered.");
        }

        return (TableInfor<T>)tableInfo;
    }

    public static TableInfor<T> Resolve<T>(string name)
    {
        if (!_namedMappings.TryGetValue((typeof(T), name), out var tableInfo))
        {
            throw new Exception($"Type {typeof(T)} with name '{name}' has not been registered.");
        }

        return (TableInfor<T>)tableInfo;
    }

    public static TableInfor<T> Resolve<T>(BulkOptions options)
    {
        return options?.MappingProfileName == null ? Resolve<T>() : Resolve<T>(options.MappingProfileName);
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

    public static bool TryResolve<T>(string name, out TableInfor<T> tableInfo)
    {
        if (_namedMappings.TryGetValue((typeof(T), name), out var info))
        {
            tableInfo = (TableInfor<T>)info;
            return true;
        }

        tableInfo = null;
        return false;
    }
}
