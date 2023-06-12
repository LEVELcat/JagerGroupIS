using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("PersonalWeaponKillStat")]
    public class PersonalWeaponKillStat
    {
        [Key]
        [Column("PdbsID")]
        public ulong ID { get; set; }

        [Column("PmsID")]
        public ulong PmsID { get; set; }

        [Column("WeaponID")]
        public ushort WeaponID { get; set; }

        [Column("Count")]
        public ushort Count { get; set; }

        public virtual PersonalMatchStat? MatchStat { get; set; }

        public virtual Weapon? Weapon { get; set; }
    }
}
