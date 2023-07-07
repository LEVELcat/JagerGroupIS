using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.StatisticModel
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

        [Column("ProfileUpdateTime")]
        public DateTime? ProfileUpdtaded { get; set; }

        public virtual ICollection<PersonalKillStat>? KillStats { get; set; }
        public virtual ICollection<PersonalDeathByStat>? DeathByStats { get; set; }

        public virtual ICollection<PersonalMatchStat>? MatchStats { get; set; }
    }
}
