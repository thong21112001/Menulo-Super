using Menulo.Application.Features.Restaurants.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menulo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IRestaurantService _resSvc;

        public ImagesController(IHttpClientFactory httpFactory, IRestaurantService resSvc)
        {
            _httpFactory = httpFactory;
            _resSvc = resSvc;
        }


        [HttpGet("restaurants/{restaurantId:int}/logo")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)] // Cache ảnh trong 1 giờ
        public async Task<IActionResult> GetRestaurantLogo(int restaurantId, CancellationToken ct)
        {
            var restaurant = await _resSvc.GetByIdAsync(restaurantId, ct);
            if (restaurant is null || string.IsNullOrWhiteSpace(restaurant.LogoUrl))
            {
                return NotFound();
            }

            try
            {
                var client = _httpFactory.CreateClient();
                var response = await client.GetAsync(restaurant.LogoUrl, ct);

                if (!response.IsSuccessStatusCode)
                {
                    // Trả về một ảnh placeholder nếu không tải được
                    return NotFound("Image from source could not be loaded.");
                }

                var imageBytes = await response.Content.ReadAsByteArrayAsync(ct);
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";

                return File(imageBytes, contentType);
            }
            catch (Exception)
            {
                // Xử lý lỗi nếu có vấn đề về mạng hoặc URL không hợp lệ
                return StatusCode(500, "Error fetching image from source.");
            }
        }
    }
}
