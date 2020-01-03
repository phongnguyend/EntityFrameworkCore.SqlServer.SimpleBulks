using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlTypeConverters
{
    public class SingleConverter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(Single);
        }

        public string Convert(Type type)
        {
            return "single";
        }
    }
}
