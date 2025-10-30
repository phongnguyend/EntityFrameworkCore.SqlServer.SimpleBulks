namespace EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;

public class TempTableOptions : BulkOptions
{
    public static readonly TempTableOptions DefaultOptions = new TempTableOptions();

    public string TableName { get; set; }

    public string PrefixName { get; set; }
}
