using Microsoft.AspNetCore.Mvc;

namespace Menulo.Controllers
{
    public class MenuItemsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
