using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.CustomSchema;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database
{
    [Table("SingleKeyRows", Schema = TestConstants.Schema)]
    public class SingleKeyRow<TId>
    {
        public TId Id { get; set; }

        public int Column1 { get; set; }

        public string Column2 { get; set; }

        public DateTime Column3 { get; set; }
    }
}
