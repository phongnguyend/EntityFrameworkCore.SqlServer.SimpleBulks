using System;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Demo.Entities
{
    public class CompositeKeyRow
    {
        public int Id1 { get; set; }
        public int Id2 { get; set; }
        public int Column1 { get; set; }
        public string Column2 { get; set; }
        public DateTime Column3 { get; set; }
    }
}
