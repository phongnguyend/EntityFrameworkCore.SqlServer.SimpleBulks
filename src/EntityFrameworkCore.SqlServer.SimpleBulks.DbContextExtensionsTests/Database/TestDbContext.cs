using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.DbContextExtensionsTests.Database;

public class TestDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly string _schema;

    public DbSet<SingleKeyRow<int>> SingleKeyRows { get; set; }

    public DbSet<CompositeKeyRow<int, int>> CompositeKeyRows { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Contact> Contacts { get; set; }

    public TestDbContext(string connectionString, string schema)
    {
        _connectionString = connectionString;
        _schema = schema;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        if (!string.IsNullOrEmpty(_schema))
        {
            modelBuilder.HasDefaultSchema(_schema);
        }

        modelBuilder.Entity<SingleKeyRow<int>>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        modelBuilder.Entity<CompositeKeyRow<int, int>>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<CompositeKeyRow<int, int>>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");

        modelBuilder.Entity<Customer>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        modelBuilder.Entity<Customer>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        modelBuilder.Entity<Contact>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        modelBuilder.Entity<Contact>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        base.OnModelCreating(modelBuilder);
    }
}
