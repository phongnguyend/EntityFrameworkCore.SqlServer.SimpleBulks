namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;

public class SingleKeyRow<TId>
{
    public TId Id { get; set; }

    public int Column1 { get; set; }

    public string Column2 { get; set; }

    public DateTime Column3 { get; set; }

    public Season? Season { get; set; }

    public Season? SeasonAsString { get; set; }

    public bool? NullableBool { get; set; }

    public DateTime? NullableDateTime { get; set; }

    public DateTimeOffset? NullableDateTimeOffset { get; set; }

    public decimal? NullableDecimal { get; set; }

    public double? NullableDouble { get; set; }

    public Guid? NullableGuid { get; set; }

    public short? NullableShort { get; set; }

    public int? NullableInt { get; set; }

    public long? NullableLong { get; set; }

    public float? NullableFloat { get; set; }

    public string? NullableString { get; set; }
}
