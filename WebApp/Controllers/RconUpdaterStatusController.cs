using DbLibrary.DbContexts;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class RconUpdaterStatusController : Controller
    {
        public string GetInfo()
        {
            string result = string.Empty;

            using (StatisticDbContext context = new StatisticDbContext())
            {
                result = context.Servers.First().Description;
                context.Dispose();
            }
            return result;
        }
    }
}
