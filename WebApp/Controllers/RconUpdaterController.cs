using Microsoft.AspNetCore.Mvc;
using WebApp.DbContexts;

namespace WebApp.Controllers
{
    public class RconUpdaterController : Controller
    {
        public IActionResult UpdateStatDb([FromServices] StatisticDbContext dbContext)
        {
            return View(Json(dbContext.Servers));
        }

        public string Check()
        {
            return "Hello Updater!!";
        }
    }
}
