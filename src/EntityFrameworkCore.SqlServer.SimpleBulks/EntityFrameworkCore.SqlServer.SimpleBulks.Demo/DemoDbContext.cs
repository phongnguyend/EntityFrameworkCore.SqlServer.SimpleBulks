using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    public class DemoDbContext : DbContext
    {
        public DbSet<Row> Rows { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=SimpleBulks;User Id=sa;Password=sqladmin123!@#;MultipleActiveResultSets=true");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
