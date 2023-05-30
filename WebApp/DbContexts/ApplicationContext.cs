using Microsoft.EntityFrameworkCore;

namespace WebApp.DbContexts
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext() 
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql("server=127.0.0.1:3306;user=remoteAdmin;password=;database=statDB;",
                new MySqlServerVersion(new Version(8, 0, 25)));
        }

    }
}
