using System;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public class DefaultConverter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return true;
        }

        public string Convert(Type type)
        {
            return "nvarchar(max)";
        }
    }
}
