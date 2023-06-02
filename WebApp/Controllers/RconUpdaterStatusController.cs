using Microsoft.AspNetCore.Mvc;
using WebApp.DbContexts;
using WebApp.Services.RconScanerService;

namespace WebApp.Controllers
{
    public class RconUpdaterStatusController : Controller
    {
        public IActionResult UpdateStatDb([FromServices] RconUpdaterService service)
        {


            return null;
        }
    }
}
