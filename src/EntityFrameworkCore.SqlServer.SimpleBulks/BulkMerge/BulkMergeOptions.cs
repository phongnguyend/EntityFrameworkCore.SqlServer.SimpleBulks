namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkMerge
{
    public class BulkMergeOptions : BulkOptions
    {
        public bool WithHoldLock { get; set; }
    }
}
