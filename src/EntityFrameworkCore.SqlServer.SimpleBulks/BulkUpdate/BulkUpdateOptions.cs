namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkUpdate;

public class BulkUpdateOptions : BulkOptions
{
    public static readonly BulkUpdateOptions DefaultOptions = new BulkUpdateOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;
}
