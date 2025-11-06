namespace EntityFrameworkCore.SqlServer.SimpleBulks;

public class OutputId
{
    public string Name { get; init; }

    public OutputIdMode Mode { get; init; }
}

public enum OutputIdMode
{
    ClientGenerated,
    ServerGenerated
}
