using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.JagerDsModel
{
    [Table("rolesetap")]
    public class RoleSetup
    {
        [Key]
        [Column("RolesSetupID")]
        public uint ID { get; set; }

        [Column("ElectionID")]
        public uint ElectionID { get; set; }
        public virtual Election? Election { get; set; }

        [Column("RolesID")]
        public ushort RolesID { get; set; }
        public virtual Role? Roles { get; set; }

        [Column("IsTakingPart")]
        public bool IsTakingPart { get; set; }
    }

}
