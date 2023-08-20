using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbLibrary.MetricsModel
{
    public class MetricsDbContext : DbContext
    {
        public DbSet<LinkToSteamID> LinksWithSteam { get; set; }

        public DbSet<TrackingMessage> TrackingMessages { get; set; }



        public MetricsDbContext()
        {
            Database.EnsureCreated();
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

#if DEBUG
            //DEBUG
            optionsBuilder.UseMySql("server=localhost;user=root;password=0451;database=metricdb;",
                new MySqlServerVersion(new Version(8, 0, 25)));

#else
            //RELESE

            //IF IT'S RELEASE BUILD ON LOCAL DB
            //
            //optionsBuilder.UseMySql("server=localhost;user=root;password=0451;database=metricdb;",
            //    new MySqlServerVersion(new Version(8, 0, 25)));

            optionsBuilder.UseMySql("server=localhost;user=remoteAdmin;password=67%8#yG*isOp;database=metricdb;",
                  new MySqlServerVersion(new Version(8, 0, 25)));
#endif
        }
    }
}
