using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    public class DemoDbContext : DbContext
    {
        private const string _connectionString = "Server=.;Database=SimpleBulks;User Id=sa;Password=sqladmin123!@#";

        public DbSet<Row> Rows { get; set; }

        public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

        public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

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

            base.OnModelCreating(modelBuilder);
        }
    }
}
