using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.JagerDsModel
{
    [Table("bot_speech_for_bad_guy")]
    public class TextForBadGuy
    {
        [Key]
        [Column("ID")]
        public ushort ID { get; set; }

        [Column("BotSpeech")]
        public string BotSpeech { get; set; }
    }
}
