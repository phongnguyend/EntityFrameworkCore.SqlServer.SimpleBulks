using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleBulkOperations.Demo
{
    public class DemoDbContext : DbContext
    {
        public DbSet<Row> Rows { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=SimpleBulkOperations;User Id=sa;Password=sqladmin123!@#;MultipleActiveResultSets=true");
            base.OnConfiguring(optionsBuilder);
        }
    }
}
