using System;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public class DecimalConverter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(decimal);
        }

        public string Convert(Type type)
        {
            return "decimal(38, 20)";
        }
    }
}
