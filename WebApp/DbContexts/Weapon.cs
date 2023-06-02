using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("Weapon")]
    public class Weapon
    {
        [Key]
        [Column("WeaponID")]
        public ushort ID { get; private set; }

        [Column("Name")]
        public string WeaponName { get; set; }

        public ICollection<PersonalWeaponKillStat> WeaponKillStats { get; set; }
        public ICollection<PersonalDeathByWeaponStat> DeathByWeaponStats { get; set; }

    }
}
