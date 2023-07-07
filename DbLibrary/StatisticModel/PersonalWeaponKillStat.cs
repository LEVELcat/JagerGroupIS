using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.StatisticModel
{
    [Table("personalweaponkillstat")]
    public class PersonalWeaponKillStat
    {
        [Key]
        [Column("PwksID")]
        public ulong ID { get; set; }

        [Column("PmsID")]
        public ulong PersonalMatchStatID { get; set; }
        public virtual PersonalMatchStat? PersonalMatchStat { get; set; }

        [Column("WeaponID")]
        public ushort WeaponID { get; set; }
        public virtual Weapon? Weapon { get; set; }

        [Column("Count")]
        public ushort Count { get; set; }
    }
}
