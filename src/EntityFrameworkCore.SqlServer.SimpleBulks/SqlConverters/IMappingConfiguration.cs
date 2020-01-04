using System.Collections.Generic;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.SqlConverters
{
    public interface IMappingConfiguration<T>
    {
        string TableName { get; }

        Dictionary<string, string> PropsToColumns { get; }
    }
}
