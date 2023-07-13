using Microsoft.EntityFrameworkCore;

namespace DbLibrary.StatisticModel
{
    public class StatisticDbContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<PersonalWeaponKillStat> PerosnalWeaponKillStats { get; set; }
        public DbSet<PersonalDeathByStat> PersonalDeathByStats { get; set; }
        public DbSet<PersonalDeathByWeaponStat> PersonalDeathByWeaponStats { get; set; }
        public DbSet<PersonalKillStat> PersonalKillStats { get; set; }
        public DbSet<PersonalMatchStat> PersonalMatchStats { get; set; }
        public DbSet<ServerMatch> ServerMatches { get; set; }
        public DbSet<SteamProfile> SteamProfiles { get; set; }
        public DbSet<Weapon> Weapons { get; set; }
        public DbSet<ServerType> ServerTypes { get; set; }
        public DbSet<ServerGroup> ServerGroups { get; set; }

        public StatisticDbContext() 
        {
            Database.EnsureCreated();

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
#if DEBUG
            //DEBUG
            optionsBuilder.UseMySql("server=localhost;user=root;password=0451;database=statDB;",
                new MySqlServerVersion(new Version(8, 0, 25)));

#else
            //RELESE

            //IF IT'S RELEASE BUILD ON LOCAL DB
            //
            //optionsBuilder.UseMySql("server=localhost;user=root;password=0451;database=statDB;",
            //    new MySqlServerVersion(new Version(8, 0, 25)));

            optionsBuilder.UseMySql("server=localhost;user=remoteAdmin;password=67%8#yG*isOp;database=statdb;",
                  new MySqlServerVersion(new Version(8, 0, 25)));
#endif
        }
    }
}
