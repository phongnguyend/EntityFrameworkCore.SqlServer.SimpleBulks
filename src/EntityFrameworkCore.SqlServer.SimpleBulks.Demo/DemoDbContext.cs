using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    public class DemoDbContext : DbContext
    {
        private readonly string _connectionString;

        public DemoDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Row> Rows { get; set; }

        public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompositeKeyRow>().HasKey(x => new { x.Id1, x.Id2 });

            base.OnModelCreating(modelBuilder);
        }
    }
}
