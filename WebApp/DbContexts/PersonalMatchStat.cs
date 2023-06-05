using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("PersonalMatchStat")]
    public class PersonalMatchStat
    {
        [Key]
        [Column("PmsID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong PmsID { get; set; }

        [Column("ServerMatchID")]
        public uint MatchID { get; set; }

        [Column("SteamProfileID")]
        public uint SteamProfileID { get; set; }

        [Column("CombatPoint")]
        public ushort? Combat { get; set; }

        [Column("Deaths")]
        public ushort Deaths { get; set; }

        [Column("DeathsByTK")]
        public ushort DeathsByTK { get; set; }

        [Column("DeathsWithoutKillStreak")]
        public ushort DeathsWithoutKillStreak { get; set; }

        [Column("DefensePoint")]
        public ushort? Defense { get; set; }

        [Column("Kills")]
        public ushort Kills { get; set; }

        [Column("KillsStreak")]
        public ushort KillStreak { get; set; }

        [Column("LongestLifeSeconds")]
        public ushort LongestLife { get; set; }

        [Column("OffensePoint")]
        public ushort? Offensive { get; set; }

        [Column("ShortestLifeSecond")]
        public ushort ShortestLife { get; set; }

        [Column("Support")]
        public ushort? Support { get;set; }

        [Column("TeamKills")]
        public ushort TeamKills { get; set; }

        [Column("PlayTimeSeconds")]
        public ushort PlayTime { get; set; }

        public virtual ServerMatch? Match { get; set; }

        public virtual SteamProfile? SteamProfile { get; set; }

        public virtual List<PersonalKillStat>? KillStats { get; set; }

        public virtual List<PersonalDeathByStat>? DeathByStats { get; set; }

        public virtual List<PersonalWeaponKillStat>? WeaponKillStats { get; set; }

        public virtual List<PersonalDeathByWeaponStat>? DeathByWeaponStats { get; set; }

    }
}
