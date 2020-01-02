using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlTypeConverters
{
    public interface ISqlTypeConvertible
    {
        bool CanConvert(Type type);
        string Convert(Type type);
    }
}
