using EntityFramework.SqlServer.SimpleBulks.Demo.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace EntityFramework.SqlServer.SimpleBulks.Demo
{
    internal class DemoDbContext : DbContext
    {
        public DemoDbContext() : base("name=DefaultDb")
        {

        }

        public DbSet<Row> Rows { get; set; }

        public DbSet<CompositeKeyRow> CompositeKeyRows { get; set; }

        public DbSet<ConfigurationEntry> ConfigurationEntries { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompositeKeyRow>().HasKey(x => new { x.Id1, x.Id2 });
            modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Key).HasColumnName("Key1");
            modelBuilder.Entity<ConfigurationEntry>().Property(x => x.Id).HasColumnName("Id1");

            base.OnModelCreating(modelBuilder);
        }
    }
}
