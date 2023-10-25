using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database
{
    internal class TestDbContext : DbContext
    {
        private readonly string _connectionString;

        public DbSet<SingleKeyRow<int>> SingleKeyRows { get; set; }

        public DbSet<CompositeKeyRow<int, int>> CompositeKeyRows { get; set; }

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
            modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");
            modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");

            base.OnModelCreating(modelBuilder);
        }
    }
}
