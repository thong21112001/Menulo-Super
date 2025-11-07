using Menulo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menulo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IImageProcessingService _imageSvc;

        public ImagesController(IImageProcessingService imageSvc)
        {
            _imageSvc = imageSvc;
        }


        [HttpGet("restaurants/{restaurantId:int}/logo")]
        [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)] // Cache 1 ngày
        public async Task<IActionResult> GetRestaurantLogo(int restaurantId, [FromQuery] int? w, [FromQuery] int? h, CancellationToken ct)
        {
            try
            {
                var imageBytes = await _imageSvc.GetOrProcessRestaurantLogoAsync(restaurantId, w, h, ct);
                return File(imageBytes, "image/jpeg");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                // Có thể trả về 1 ảnh placeholder mặc định ở đây
                return StatusCode(500);
            }
        }


        [HttpGet("menuitems/{menuItemId:int}")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
        [AllowAnonymous] // nếu menu cho khách bên ngoài truy cập (tuỳ flow), else giữ [Authorize]
        public async Task<IActionResult> GetMenuItemImage(int menuItemId, [FromQuery] int? w, [FromQuery] int? h, CancellationToken ct)
        {
            try
            {
                var imageBytes = await _imageSvc.GetOrProcessMenuItemImageAsync(menuItemId, w, h, ct);
                return File(imageBytes, "image/jpeg");
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

    }
}
