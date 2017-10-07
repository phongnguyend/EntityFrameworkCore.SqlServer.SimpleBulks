using System;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public class DoubleConverter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(double);
        }

        public string Convert(Type type)
        {
            return "double";
        }
    }
}
