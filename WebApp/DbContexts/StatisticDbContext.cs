using Microsoft.EntityFrameworkCore;

namespace WebApp.DbContexts
{
    public class StatisticDbContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Map> Maps { get; set; }
        public DbSet<PerosnalWeaponKillStat> PerosnalWeaponKillStats { get; set; }
        public DbSet<PersonalDeathByStat> PersonalDeathByStats { get; set; }
        public DbSet<PersonalDeathByWeaponStat> PersonalDeathByWeaponStats { get; set; }
        public DbSet<PersonalKillStat> PersonalKillStats { get; set; }
        public DbSet<PersonalMatchStat> PersonalMatchStats { get; set; }
        public DbSet<ServerMatch> ServerMatches { get; set; }
        public DbSet<SteamProfile> SteamProfiles { get; set; }
        public DbSet<Weapon> Weapons { get; set; }

        public StatisticDbContext() 
        {
            Database.EnsureCreated();

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PerosnalWeaponKillStat>().HasKey(k => new { k.WeaponID, k.PmsID });
            modelBuilder.Entity<PersonalDeathByWeaponStat>().HasKey(k => new { k.WeaponID, k.PmsID });
            modelBuilder.Entity<PersonalKillStat>().HasKey(k => new { k.SteamProfileID, k.PmsID });
            modelBuilder.Entity<PersonalDeathByStat>().HasKey(k => new { k.SteamProfileID, k.PmsID });


            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
#if DEBUG
            //DEBUG
            optionsBuilder.UseMySql("server=localhost;user=root;password=0451;database=statDB;",
                new MySqlServerVersion(new Version(8, 0, 25)));

#else
            //RELESE
            optionsBuilder.UseMySql("server=127.0.0.1:3306;user=remoteAdmin;password=;database=statDB;",
                new MySqlServerVersion(new Version(8, 0, 25)));
#endif
        }
    }
}
