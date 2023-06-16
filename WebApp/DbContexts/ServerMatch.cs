﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("servermatch")]
    public class ServerMatch
    {
        [Key]
        [Column("ServerMatchID")]
        public uint ID { get; set; }

        [Column("ServerID")]
        public uint ServerID { get; set; }
        public virtual Server? Server { get; set; }

        [Column("MapID")]
        public ushort MapID { get; set; }
        public virtual Map? Map { get; set; }

        [Column("ServerLocalMatchID")]
        public uint ServerLocalMatchId { get; set; }

        [Column("StartTime")]
        public DateTime StartTime { get; set; }

        [Column("EndTime")]
        public  DateTime EndTime { get; set; }

        [Column("CreationTime")]
        public DateTime CreationTime { get; set; }

        public virtual List<PersonalMatchStat>? PersonalsMatchStat { get; set; }
    }
}
