using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.MetricsModel
{
    [Table("link_to_steam64")]
    public class LinkToSteamID
    {
        [Key]
        [Column("ID")]
        public ushort ID { get; set; }

        [Column("DiscrodMemberID")]
        public uint DiscordMemberID { get; set; }

        [Column("Steam64ID")]
        public uint Steam64ID { get; set; }
    }
}
