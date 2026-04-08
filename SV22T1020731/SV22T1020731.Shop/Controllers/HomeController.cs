using Microsoft.AspNetCore.Mvc;

namespace SV22T1020731.Shop.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
