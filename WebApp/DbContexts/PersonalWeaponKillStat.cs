﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("PersonalWeaponKillStat")]
    public class PersonalWeaponKillStat
    {
        [Column("PmsID")]
        public ulong PmsID { get; set; }

        [Column("WeaponID")]
        public ushort WeaponID { get; set; }

        [Column("Count")]
        public ushort Count { get; set; }

        public virtual PersonalMatchStat? MatchStat { get; set; }

        public virtual Weapon? Weapon { get; set; }
    }
}
