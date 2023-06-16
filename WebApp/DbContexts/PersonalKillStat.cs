using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("personalkillstat")]
    public class PersonalKillStat
    {
        [Key]
        [Column("PksID")]
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
