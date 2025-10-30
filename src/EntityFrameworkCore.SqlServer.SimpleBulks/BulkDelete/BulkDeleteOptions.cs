namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete;

public class BulkDeleteOptions : BulkOptions
{
    public static readonly BulkDeleteOptions DefaultOptions = new BulkDeleteOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;
}
