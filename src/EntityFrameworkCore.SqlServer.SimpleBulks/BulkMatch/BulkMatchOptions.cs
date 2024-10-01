namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;

public class BulkMatchOptions : BulkOptions
{
    public string Collation { get; set; } = Constants.DefaultCollation;
}
