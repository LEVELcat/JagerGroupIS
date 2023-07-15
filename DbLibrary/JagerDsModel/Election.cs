using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.JagerDsModel
{
    [Table("election")]
    public class Election
    {
        [Key]
        [Column("ElectionID")]
        public uint ID { get; set; }

        [Column("StartTime")]
        public DateTime StartTime { get; set; }

        [Column("EndTime")]
        public DateTime? EndTime { get; set; }

        [Column("GuildID")]
        public ulong GuildID { get; set; }

        [Column("ChanelID")]
        public ulong ChanelID { get; set; }

        [Column("MessageID")]
        public ulong MessageID { get; set; }

        [Column("SettingBitMask")]
        public BitMaskElection BitMaskSettings { get; set; }
    }
}
