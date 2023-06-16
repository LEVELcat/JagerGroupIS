using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("personaldeathbystat")]
    public class PersonalDeathByStat
    {
        [Key]
        [Column("PdbsID")]
        public ulong ID { get; set; }

        [Column("PmsID")]
        public ulong PersonalMatchStatID { get; set; }
        public virtual PersonalMatchStat? PersonalMatchStat { get; set; }

        [Column("SteamProfileID")]
        public uint SteamProfileID { get; set; }
        public virtual SteamProfile? SteamProfile { get; set; }

        [Column("Count")]
        public ushort Count { get; set; }
    }
}
