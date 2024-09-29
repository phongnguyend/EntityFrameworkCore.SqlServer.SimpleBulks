using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.SqlServer.SimpleBulks
{
    public static class TableMapper
    {
        private static readonly object _lock = new object();
        private static Dictionary<Type, (string Schema, string TableName)> _mappings = new Dictionary<Type, (string Schema, string TableName)>();

        public static void Register(Type type, string schema, string tableName)
        {
            lock (_lock)
            {
                _mappings[type] = (Schema: schema, TableName: tableName);
            }
        }

        public static string Resolve(Type type)
        {
            if (!_mappings.Keys.Contains(type))
            {
                throw new Exception($"Type {type} has not been registered.");
            }

            var map = _mappings[type];

            return string.IsNullOrEmpty(map.Schema) ? $"[{map.TableName}]" : $"[{map.Schema}].[{map.TableName}]";
        }
    }
}
