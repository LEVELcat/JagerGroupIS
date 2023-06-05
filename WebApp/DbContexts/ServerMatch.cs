using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("ServerMatch")]
    public class ServerMatch
    {
        [Key]
        [Column("ServerMatchID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint SMID { get; set; }

        [Column("ServerID")]
        public uint ServerID { get; set; }

        [Column("MapID")]
        public ushort MapID { get; set; }

        [Column("ServerLocalMatchID")]
        public uint ServerLocalMatchId { get; set; }

        [Column("StartTime")]
        public DateTime StartTime { get; set; }

        [Column("EndTime")]
        public  DateTime EndTime { get; set; }

        [Column("CreationTime")]
        public DateTime CreationTime { get; set; }

        public virtual Server? Server { get; set; }

        public virtual Map? Map { get; set; }

        public virtual List<PersonalMatchStat>? PersonalsMatchStat { get; set; }
    }
}
