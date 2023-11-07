using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database
{
    public class SingleKeyRow<TId>
    {
        public TId Id { get; set; }

        public int Column1 { get; set; }

        public string Column2 { get; set; }

        public DateTime Column3 { get; set; }
    }

    [Table("SingleKeyRows", Schema = "test")]
    public class SingleKeyRowWithSchema<TId>
    {
        public TId Id { get; set; }

        public int Column1 { get; set; }

        public string Column2 { get; set; }

        public DateTime Column3 { get; set; }
    }
}
