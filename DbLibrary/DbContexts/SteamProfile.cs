using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.DbContexts
{
    [Table("steamprofile")]
    public class SteamProfile
    {
        [Key]
        [Column("SteamProfileID")]
        public uint ID { get; set; }

        [Column("SteamID64")]
        public ulong SteamID64 { get; set; }

        [Column("NickName")]
        public string? SteamName { get; set; }

        [Column("AvatarHash")]
        public string? AvatarHash { get; set; }

        public virtual IEnumerable<PersonalKillStat>? KillStats { get; set; }
        public virtual IEnumerable<PersonalDeathByStat>? DeathByStats { get; set; }

        public virtual IEnumerable<PersonalMatchStat>? MatchStats { get; set; }
    }
}
