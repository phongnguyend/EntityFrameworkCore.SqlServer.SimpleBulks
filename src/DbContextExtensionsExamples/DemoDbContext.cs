using DbContextExtensionsExamples.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbContextExtensionsExamples;

public class DemoDbContext : DbContext
{
    private string _connectionString = ConnectionStrings.SqlServerConnectionString;

    public DbSet<Row> Rows { get; set; }

    public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

    public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

    public DbSet<Order> Orders { get; set; }

    public DbSet<Blog> Blogs { get; set; }

    public DbSet<RssBlog> RssBlogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompositeKeyRow>().HasKey(x => new { x.Id1, x.Id2 });
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.SeasonAsString).HasConversion(v => v.ToString(), v => (Season)Enum.Parse(typeof(Season), v));

        base.OnModelCreating(modelBuilder);
    }
}

public class Order
{
    public int Id { get; set; }

    [Required]
    public Address ShippingAddress { get; set; }
}

[ComplexType]
public class Address
{
    public string Street { get; set; }

    [Required]
    public Location Location { get; set; }
}

[ComplexType]
public class Location
{
    public double Lat { get; set; }

    public double Lng { get; set; }
}

public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }
}

public class RssBlog : Blog
{
    public string RssUrl { get; set; }
}