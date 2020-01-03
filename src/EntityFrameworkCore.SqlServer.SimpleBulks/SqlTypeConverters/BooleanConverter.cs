using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlTypeConverters
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
