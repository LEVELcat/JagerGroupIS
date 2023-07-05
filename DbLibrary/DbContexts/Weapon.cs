using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.DbContexts
{
    [Table("weapon")]
    public class Weapon
    {
        [Key]
        [Column("WeaponID")]
        public ushort ID { get; set; }

        [Column("Name")]
        public string WeaponName { get; set; }

        public virtual ICollection<PersonalWeaponKillStat>? WeaponKillStats { get; set; }
        public virtual ICollection<PersonalDeathByWeaponStat>? DeathByWeaponStats { get; set; }

    }
}
