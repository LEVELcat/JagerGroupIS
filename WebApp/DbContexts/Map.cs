using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

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

        public ICollection<Match> Matches { get; set; }
    }
}
