namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkSelect
{
    public class BulkSelectOptions : BulkOptions
    {
        public string Collation { get; set; } = Constants.DefaultCollation;
    }
}
