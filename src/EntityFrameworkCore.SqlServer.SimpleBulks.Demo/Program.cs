using EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities;
using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            DbContextTest.Run();
            SqlConnectionTest.Run();

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }
    }
}
