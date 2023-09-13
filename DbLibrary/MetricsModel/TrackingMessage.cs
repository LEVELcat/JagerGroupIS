using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.MetricsModel
{
    [Table("tracking_message")]
    public class TrackingMessage
    {
        [Key]
        [Column("ID")]
        public uint ID { get; set; }

        [Column("GuildID")]
        public ulong GuildID { get; set; }

        [Column("ChanelID")]
        public ulong ChanelID { get; set; }

        [Column("MessageID")]
        public ulong MessageID { get; set; }

        [Column("MessageTypeID")]
        public MessageType MessageType { get; set; }
    }
}
