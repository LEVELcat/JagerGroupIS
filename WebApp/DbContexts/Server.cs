using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("server")]
    public class Server
    {
        [Key]
        [Column("ServerID")]
        public uint ID { get; set; }

        [Column("ServerAdress")]
        public string RconURL { get; set; }

        [Column("ServerDescription")]
        public string? Description { get; set; }

        [Column("ServerIsTracking")]
        public bool ServerIsTracking { get; set; }

        [Column("ServerGroupID")]
        public ushort ServerGroupID { get; set; }

        public virtual ServerGroup? ServerGroup { get; set; }

        public virtual List<ServerMatch>? Matches { get; set; }
    }
}
