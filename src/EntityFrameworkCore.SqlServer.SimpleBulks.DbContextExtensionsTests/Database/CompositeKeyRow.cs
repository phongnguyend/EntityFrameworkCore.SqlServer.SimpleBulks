namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;

public class CompositeKeyRow<TId1, TId2>
{
    public TId1 Id1 { get; set; }

    public TId2 Id2 { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    public DateTime Column3 { get; set; }

    public Season? Season { get; set; }

    public Season? SeasonAsString { get; set; }
}
