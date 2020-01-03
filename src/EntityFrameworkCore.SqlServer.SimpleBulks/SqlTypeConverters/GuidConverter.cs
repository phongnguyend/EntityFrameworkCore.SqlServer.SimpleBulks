using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlTypeConverters
{
    public class GuidConverter : ISqlTypeConvertible
    {
        public bool CanConvert(Type type)
        {
            return type == typeof(Guid);
        }

        public string Convert(Type type)
        {
            return "uniqueidentifier";
        }
    }
}
