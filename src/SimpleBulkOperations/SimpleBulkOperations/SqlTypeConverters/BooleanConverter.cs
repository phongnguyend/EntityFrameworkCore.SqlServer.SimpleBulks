using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public class BooleanConverter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(bool);
        }

        public string Convert(Type type)
        {
            return "bit";
        }
    }
}
