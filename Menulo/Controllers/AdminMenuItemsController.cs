using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.MenuItems.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menulo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin/menu-items")]
    public class AdminMenuItemsController : Controller
    {
        private readonly ICurrentUser _current;
        private readonly IMenuItemsService _svc;

        public AdminMenuItemsController(ICurrentUser current, IMenuItemsService svc)
        {
            _current = current;
            _svc = svc;
        }


        // API 1: GET /api/admin/menu-items/simple
        /// <summary>
        /// Lấy danh sách món ăn đơn giản cho dropdown
        /// </summary>
        [HttpGet("simple")]
        public async Task<IActionResult> GetSimple(CancellationToken ct)
        {
            var restaurantId = _current.RestaurantId ?? 0;
            if (restaurantId == 0) return Unauthorized();

            var items = await _svc.GetSimpleAsync(restaurantId, ct);
            return Ok(items);
        }
    }
}
