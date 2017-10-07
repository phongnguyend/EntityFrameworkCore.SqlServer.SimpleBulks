using System;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public interface ISqlTypeConvertible
    {
        bool CanConvert(Type type);
        string Convert(Type type);
    }
}
