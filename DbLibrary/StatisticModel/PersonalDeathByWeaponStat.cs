using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.StatisticModel
{
    [Table("personaldeathbyweaponstat")]
    public class PersonalDeathByWeaponStat
    {
        [Key]
        [Column("PdbwsID")]
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
