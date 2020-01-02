using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlTypeConverters
{
    public class Int32Converter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(Int32);
        }

        public string Convert(Type type)
        {
            return "int";
        }
    }
}
