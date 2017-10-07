using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public interface ISqlTypeConvertible
    {
        bool CanConvert(Type type);
        string Convert(Type type);
    }
}
