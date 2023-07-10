using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.StatisticModel
{
    [Table("server")]
    public class Server
    {
        [Key]
        [Column("ServerID")]
        public uint ID { get; set; }

        [Column("ServerName")]
        public string? ServerName { get; set; }

        [Column("ServerGroupID")]
        public ushort ServerGroupID { get; set; }
        public virtual ServerGroup? ServerGroup { get; set; }

        [Column("ServerNumber")]
        public byte ServerNumber { get; set; }

        [Column("ServerTypeID")]
        public ushort? ServerTypeID { get; set; }
        public virtual ServerType? ServerType { get; set; }




        public virtual ICollection<ServerMatch>? Matches { get; set; }
    }
}
