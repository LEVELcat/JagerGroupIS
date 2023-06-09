﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DbLibrary.StatisticModel
{
    [Table("map")]
    public class Map
    {
        [Key]
        [Column("MapID")]
        public ushort ID { get; set; }

        [Column("MapName")]
        public string MapName { get; set; }

        public virtual  ICollection<ServerMatch>? Matches { get; set; }
    }
}
