using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("PersonalMatchStat")]
    public class PersonalMatchStat
    {
        [Key]
        [Column("PmsID")]
        public ulong ID { get; private set; }

        [Column("ServerMatchID")]
        public uint MatchID { get; private set; }

        [Column("SteamProfileID")]
        public uint SteamProfileID { get; private set; }

        [Column("CombatPoint")]
        public ushort Combat { get; set; }

        [Column("Deaths")]
        public ushort Deaths { get; set; }

        [Column("DeathsByTK")]
        public ushort DeathsByTK { get; set; }

        [Column("DeathsWithoutKillStreak")]
        public ushort DeathsWithoutKillStreak { get; set; }

        [Column("DefensePoint")]
        public ushort Defense { get; set; }

        [Column("Kills")]
        public ushort Kills { get; set; }

        [Column("KillsStreak")]
        public ushort KillStreak { get; set; }

        [Column("LongestLifeSeconds")]
        public ushort LongestLife { get; set; }

        [Column("OffensePoint")]
        public ushort Offensive { get; set; }

        [Column("ShortestLifeSecond")]
        public ushort ShortestLife { get; set; }

        [Column("Support")]
        public ushort Support { get;set; }

        [Column("TeamKills")]
        public ushort TeamKills { get; set; }

        [Column("PlayTimeSeconds")]
        public ushort PlayTime { get; set; }

        public ServerMatch Match { get; set; }

        public SteamProfile SteamProfile { get; set; }

        public ICollection<PersonalKillStat> KillStats { get; set; }

        public ICollection<PersonalDeathByStat> DeathByStats { get; set; }

        public ICollection<PerosnalWeaponKillStat> WeaponKillStats { get; set; }

        public ICollection<PersonalDeathByWeaponStat> DeathByWeaponStats { get; set; }

    }
}
