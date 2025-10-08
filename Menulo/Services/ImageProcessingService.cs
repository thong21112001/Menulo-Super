using Menulo.Application.Features.Restaurants.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Menulo.Services
{
    // Interface để đăng ký DI
    public interface IImageProcessingService
    {
        Task<byte[]> GetOrProcessRestaurantLogoAsync(int restaurantId, int? width, int? height, CancellationToken ct);
    }

    public class ImageProcessingService : IImageProcessingService
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IRestaurantService _resSvc;

        public ImageProcessingService(IMemoryCache cache, IHttpClientFactory httpFactory, IRestaurantService resSvc)
        {
            _cache = cache;
            _httpFactory = httpFactory;
            _resSvc = resSvc;
        }

        public async Task<byte[]> GetOrProcessRestaurantLogoAsync(int restaurantId, int? width, int? height, CancellationToken ct)
        {
            // Tạo một key cache duy nhất dựa trên ID, chiều rộng và chiều cao
            string cacheKey = $"logo-{restaurantId}-{width ?? 0}-{height ?? 0}";

            // 1. Thử lấy ảnh từ cache trước
            if (_cache.TryGetValue(cacheKey, out byte[]? imageBytes) && imageBytes is not null)
            {
                return imageBytes; // Trả về ngay lập tức nếu có trong cache
            }

            // 2. Nếu không có trong cache, lấy URL từ database
            var restaurant = await _resSvc.GetByIdAsync(restaurantId, ct)
                             ?? throw new FileNotFoundException("Restaurant not found.");

            if (string.IsNullOrWhiteSpace(restaurant.LogoUrl))
            {
                throw new FileNotFoundException("Restaurant does not have a logo.");
            }

            // 3. Tải ảnh gốc từ Google Drive
            var client = _httpFactory.CreateClient();
            var response = await client.GetAsync(restaurant.LogoUrl, ct);
            response.EnsureSuccessStatusCode();

            imageBytes = await response.Content.ReadAsByteArrayAsync(ct);

            // 4. Xử lý ảnh (thay đổi kích thước) nếu có yêu cầu
            if (width.HasValue || height.HasValue)
            {
                using var image = Image.Load(imageBytes);
                var resizeOptions = new ResizeOptions
                {
                    Size = new Size(width ?? 0, height ?? 0),
                    Mode = ResizeMode.Max // Giữ tỷ lệ, co lại theo chiều lớn nhất
                };
                image.Mutate(x => x.Resize(resizeOptions));

                using var ms = new MemoryStream();
                // Lưu ảnh đã xử lý với chất lượng tốt
                await image.SaveAsJpegAsync(ms, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 85 }, ct);
                imageBytes = ms.ToArray();
            }

            // 5. Lưu kết quả vào cache để dùng cho lần sau
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30)); // Cache trong 30 phút
            _cache.Set(cacheKey, imageBytes, cacheEntryOptions);

            return imageBytes;
        }
    }
}
