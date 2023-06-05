using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("Weapon")]
    public class Weapon
    {
        [Key]
        [Column("WeaponID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ushort WID { get; set; }

        [Column("Name")]
        public string WeaponName { get; set; }

        public virtual List<PersonalWeaponKillStat>? WeaponKillStats { get; set; }
        public virtual List<PersonalDeathByWeaponStat>? DeathByWeaponStats { get; set; }

    }
}
