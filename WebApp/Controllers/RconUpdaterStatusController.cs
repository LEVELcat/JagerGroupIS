using Microsoft.AspNetCore.Mvc;
using WebApp.DbContexts;
using WebApp.Services.RconScanerService;

namespace WebApp.Controllers
{
    public class RconUpdaterStatusController : Controller
    {
        public string GetInfo([FromServices] StatisticDbContext service)
        {
            service = WebApp.Application.Services.GetService<StatisticDbContext>();

            string result = service.Servers.First().Description;
            service.Dispose();
            return result;
        }
    }
}
