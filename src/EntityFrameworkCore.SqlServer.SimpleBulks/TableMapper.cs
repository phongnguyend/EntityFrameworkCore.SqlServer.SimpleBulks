using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks
{
    public static class TableMapper
    {
        private static readonly object _lock = new object();
        private static Dictionary<Type, string> _mappings = new Dictionary<Type, string>();

        public static void Register(Type type, string tableName)
        {
            lock (_lock)
            {
                _mappings[type] = tableName;
            }
        }

        public static string Resolve(Type type)
        {
            if (!_mappings.Keys.Contains(type))
            {
                throw new Exception($"Type {type} has not been registered.");
            }

            return _mappings[type];
        }
    }
}
