namespace EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;

public class BulkInsertOptions : BulkOptions
{
    public static readonly BulkInsertOptions DefaultOptions = new BulkInsertOptions();

    public bool KeepIdentity { get; set; }
}
