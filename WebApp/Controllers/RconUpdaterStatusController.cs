using DbLibrary.DbContexts;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
    public class RconUpdaterStatusController : Controller
    {
        public string GetInfo()
        {
            StatisticDbContext context = new StatisticDbContext();

            string result = context.Servers.First().Description;
            context.Dispose();
            return result;
        }
    }
}
