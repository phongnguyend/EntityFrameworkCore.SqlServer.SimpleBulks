using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks
{
    public static class TableMapper
    {
        private static readonly object _lock = new object();
        private static Dictionary<Type, TableInfor> _mappings = new Dictionary<Type, TableInfor>();

        public static void Register(Type type, string schema, string tableName)
        {
            lock (_lock)
            {
                _mappings[type] = new TableInfor(schema, tableName);
            }
        }

        public static TableInfor Resolve(Type type)
        {
            if (!_mappings.Keys.Contains(type))
            {
                throw new Exception($"Type {type} has not been registered.");
            }

            return _mappings[type];
        }
    }
}
