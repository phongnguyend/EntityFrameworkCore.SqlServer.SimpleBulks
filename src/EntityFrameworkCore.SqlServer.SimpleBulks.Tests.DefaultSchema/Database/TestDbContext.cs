﻿using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;

public class TestDbContext : DbContext
{
    private readonly string _connectionString;

    public DbSet<SingleKeyRow<int>> SingleKeyRows { get; set; }

    public DbSet<CompositeKeyRow<int, int>> CompositeKeyRows { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Contact> Contacts { get; set; }

    public TestDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_connectionString);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompositeKeyRow<int, int>>().HasKey(x => new { x.Id1, x.Id2 });

        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");
        modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");

        modelBuilder.Entity<Customer>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

        modelBuilder.Entity<Contact>().Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

        base.OnModelCreating(modelBuilder);
    }
}
