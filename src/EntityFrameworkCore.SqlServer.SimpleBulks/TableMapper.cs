using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks
{
    public static class TableMapper
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, TableInfor> _mappings = new Dictionary<Type, TableInfor>();

        public static void Register(Type type, string tableName)
        {
            Register(type, null, tableName);
        }

        public static void Register(Type type, string schema, string tableName)
        {
            lock (_lock)
            {
                _mappings[type] = new TableInfor(schema, tableName);
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
}
