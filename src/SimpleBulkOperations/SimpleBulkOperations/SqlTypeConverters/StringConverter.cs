using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBulkOperations.SqlTypeConverters
{
    public class StringConverter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(string);
        }

        public string Convert(Type type)
        {
            return "nvarchar(max)";
        }
    }
}
