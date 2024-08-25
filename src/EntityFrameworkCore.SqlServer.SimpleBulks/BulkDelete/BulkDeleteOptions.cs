namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkDelete
{
    public class BulkDeleteOptions : BulkOptions
    {
        public string Collation { get; set; } = Constants.DefaultCollation;
    }
}
