using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("PersonalDeathByWeaponStat")]
    public class PersonalDeathByWeaponStat
    {
        [Column("PmsID")]
        public ulong PmsID { get; set; }

        [Column("WeaponID")]
        public ushort WeaponID { get; set; }

        [Column("Count")]
        public ushort Count { get; set; }

        public PersonalMatchStat MatchStat { get; set; }

        public Weapon Weapon { get; set; }
    }
}
