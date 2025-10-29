using Microsoft.AspNetCore.Mvc;

namespace Menulo.Controllers
{
    public class SalesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
