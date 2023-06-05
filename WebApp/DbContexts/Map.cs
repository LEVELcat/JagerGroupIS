using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("Map")]
    public class Map
    {
        [Key]
        [Column("MapID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ushort MID { get; set; }

        [Column("MapName")]
        public string MapName { get; set; }

        public virtual List<ServerMatch>? Matches { get; set; }
    }
}
