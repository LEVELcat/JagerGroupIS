using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace WebApp.DbContexts
{
    [Table("Server")]
    public class Server
    {
        [Key]
        [Column("ServerID")]
        public uint ID { get; private set; }
        [Column("ServerAdress")]
        public string? RconURL { get; set; }
        [Column("ServerDescription")]
        public string? Description { get; set; }
        [Column("ServerIsTracking")]
        public bool ServerIsTracking { get; set; }

        public IEnumerable<ServerMatch> Matches { get; set; }
    }

    [Table("Map")]
    public class Map
    {
    }

    [Table("ServerMatch")]
    public class ServerMatch
    {
    }

    [Table("ServerMatch")]
    public class SteamProfile
    {
    }

    [Table("Weapon")]
    public class Weapon
    {
    }
    [Table("PersonalMatchStat")]
    public class PersonalMatchStat
    {
    }
    [Table("PersonalDeathByWeaponStat")]
    public class PersonalDeathByWeaponStat
    {
    }
    [Table("PersonalDeathByStat")]
    public class PersonalDeathByStat
    {
    }
    [Table("PersonalWeaponKillStat")]
    public class PerosnalWeaponKillStat
    {
    }
    [Table("PersonalKillStat")]
    public class PersonalKillStat
    {
    }


}
