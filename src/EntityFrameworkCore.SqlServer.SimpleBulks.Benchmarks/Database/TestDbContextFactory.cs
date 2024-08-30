﻿using Microsoft.EntityFrameworkCore.Design;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database
{
    internal class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext(string[] args)
        {
            return new TestDbContext("Server=.;Database=EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks;User Id=sa;Password=sqladmin123!@#");
        }
    }
}