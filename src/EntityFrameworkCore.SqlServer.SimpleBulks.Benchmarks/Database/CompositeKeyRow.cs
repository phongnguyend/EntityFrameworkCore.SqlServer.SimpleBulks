using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Benchmarks.Database
{
    public class CompositeKeyRow<TId1, TId2>
    {
        public TId1 Id1 { get; set; }

        public TId2 Id2 { get; set; }

        public int Column1 { get; set; }

        public string Column2 { get; set; }

        public DateTime Column3 { get; set; }
    }

    [Table("CompositeKeyRows", Schema = "test")]
    public class CompositeKeyRowWithSchema<TId1, TId2>
    {
        public TId1 Id1 { get; set; }

        public TId2 Id2 { get; set; }

        public int Column1 { get; set; }

        public string Column2 { get; set; }

        public DateTime Column3 { get; set; }
    }
}
