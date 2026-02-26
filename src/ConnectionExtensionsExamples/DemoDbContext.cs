using ConnectionExtensionsExamples.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace ConnectionExtensionsExamples;

public class DemoDbContext : DbContext
{
    private string _connectionString = ConnectionStrings.SqlServerConnectionString;

    public DbSet<Row> Rows { get; set; }

    public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

    public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);

#if NET9_0_OR_GREATER
        optionsBuilder.ConfigureWarnings(x => x.Log([RelationalEventId.PendingModelChangesWarning]));
#endif

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompositeKeyRow>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        base.OnModelCreating(modelBuilder);
    }
}
