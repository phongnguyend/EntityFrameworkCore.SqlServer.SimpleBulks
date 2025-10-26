using System.ComponentModel.DataAnnotations;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;

public class ConfigurationEntry
{
    public Guid Id { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    public DateTimeOffset CreatedDateTime { get; set; }

    public DateTimeOffset? UpdatedDateTime { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public string Description { get; set; }

    public bool IsSensitive { get; set; }
}
