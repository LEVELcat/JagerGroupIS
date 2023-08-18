using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.JagerDsModel
{
    [Table("bad_guy")]
    public class BadGuy
    {
        [Key]
        [Column("ID")]
        public uint ID { get; set; }

        [Column("DiscordMemberID")]
        public ulong DiscordMemberID { get; set; }

        [Column("ElectionID")]
        public uint ElectionID { get; set; }
        public virtual Election? Election { get; set; }
    }
}
