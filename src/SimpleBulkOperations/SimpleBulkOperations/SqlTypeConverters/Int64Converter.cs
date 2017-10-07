using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public class Int64Converter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(Int64);
        }

        public string Convert(Type type)
        {
            return "bigint";
        }
    }
}
