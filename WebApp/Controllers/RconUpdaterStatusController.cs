using Microsoft.AspNetCore.Mvc;
using WebApp.DbContexts;
using WebApp.Services.RconScanerService;

namespace WebApp.Controllers
{
    public class RconUpdaterStatusController : Controller
    {
        public string GetInfo()
        {
            StatisticDbContext service = new StatisticDbContext();

            string result = service.Servers.First().Description;
            service.Dispose();
            return result;
        }
    }

    public class StatisticController : Controller
    {
        public string Index() => "Hello World";



    }
}
