using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("Server")]
    public class Server
    {
        [Key]
        [Column("ServerID")]
        public uint ID { get; private set; }

        [Column("ServerAdress")]
        public string RconURL { get; set; }

        [Column("ServerDescription")]
        public string? Description { get; set; }

        [Column("ServerIsTracking")]
        public bool ServerIsTracking { get; set; }

        public ICollection<ServerMatch> Matches { get; set; }
    }
}
