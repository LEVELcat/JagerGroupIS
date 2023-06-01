using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("Map")]
    public class Map
    {
        [Key]
        [Column("MapID")]
        public ushort ID { get; private set; }

        [Column("Name")]
        public string MapName { get; set; }

        public ICollection<ServerMatch> Matches { get; set; }
    }
}
