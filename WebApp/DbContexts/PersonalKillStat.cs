using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.DbContexts
{
    [Table("PersonalKillStat")]
    public class PersonalKillStat
    {
        [Column("PmsID")]
        public ulong PmsID { get; set; }

        [Column("SteamProfileID")]
        public ushort SteamProfileID { get; set; }

        [Column("Count")]
        public ushort Count { get; set; }

        public virtual PersonalMatchStat? MatchStat { get; set; }

        public virtual SteamProfile? SteamProfile { get; set; }
    }
}
