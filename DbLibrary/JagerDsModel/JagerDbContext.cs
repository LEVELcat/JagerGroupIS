using Microsoft.EntityFrameworkCore;

namespace DbLibrary.JagerDsModel
{
    public class JagerDbContext : DbContext
    {





        public JagerDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();

#if DEBUG
            //DEBUG
            optionsBuilder.UseMySql("server=localhost;user=root;password=0451;database=jagerdb;",
                new MySqlServerVersion(new Version(8, 0, 25)));

#else
            //RELESE

            //IF IT'S RELEASE BUILD ON LOCAL DB
            //
            //optionsBuilder.UseMySql("server=localhost;user=root;password=0451;database=jagerdb;",
            //    new MySqlServerVersion(new Version(8, 0, 25)));

            optionsBuilder.UseMySql("server=localhost;user=remoteAdmin;password=67%8#yG*isOp;database=jagerdb;",
                  new MySqlServerVersion(new Version(8, 0, 25)));
#endif
        }
    }
}
