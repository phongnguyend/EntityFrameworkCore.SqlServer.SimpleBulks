namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge;

public class BulkMergeOptions : BulkOptions
{
    public static readonly BulkMergeOptions DefaultOptions = new BulkMergeOptions();

    public string Collation { get; set; } = Constants.DefaultCollation;

    public bool WithHoldLock { get; set; }

    public bool ReturnDbGeneratedId { get; set; } = true;
}
