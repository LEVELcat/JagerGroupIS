using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.JagerDsModel
{
    [Table("roles")]
    public class Roles
    {
        [Key]
        [Column("RolesID")]
        public ushort ID { get; set; }

        [Column("RolesUlong")]
        public ulong RoleDiscordID { get; set; }

        [Column("GuildID")]
        public ulong GuildID { get; set; }

        public virtual ICollection<RoleSetup>? RoleSetups { get; set; }
    }
}
