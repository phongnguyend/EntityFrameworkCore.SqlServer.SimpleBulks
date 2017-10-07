using System;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public class Int16Converter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(Int16);
        }

        public string Convert(Type type)
        {
            return "smallint";
        }
    }
}
