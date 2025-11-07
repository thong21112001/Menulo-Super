using Google.Apis.Drive.v3.Data;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Text.RegularExpressions;

namespace Menulo.Services
{
    // Interface để đăng ký DI
    public interface IImageProcessingService
    {
        Task<byte[]> GetOrProcessRestaurantLogoAsync(int restaurantId, int? width, int? height, CancellationToken ct);
        Task<byte[]> GetOrProcessMenuItemImageAsync(int menuItemId, int? w, int? h, CancellationToken ct);
    }



    public class ImageProcessingService : IImageProcessingService
    {
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IRestaurantService _resSvc;
        private readonly IUnitOfWork _uow;

        public ImageProcessingService(IMemoryCache cache, IHttpClientFactory httpFactory, 
            IRestaurantService resSvc, IUnitOfWork uow)
        {
            _cache = cache;
            _httpFactory = httpFactory;
            _resSvc = resSvc;
            _uow = uow;
        }

        public async Task<byte[]> GetOrProcessRestaurantLogoAsync(int restaurantId, int? width, int? height, CancellationToken ct)
        {
            // Tạo một key cache duy nhất dựa trên ID, chiều rộng và chiều cao
            string cacheKey = $"logo-{restaurantId}-{width ?? 0}-{height ?? 0}";

            // 1. Thử lấy ảnh từ cache trước
            if (_cache.TryGetValue(cacheKey, out byte[]? cached) && cached is not null)
                return cached;

            // 2. Nếu không có trong cache, lấy URL từ database
            var restaurant = await _resSvc.GetByIdAsync(restaurantId, ct)
                             ?? throw new FileNotFoundException("Restaurant not found.");

            if (string.IsNullOrWhiteSpace(restaurant.LogoUrl))
            {
                throw new FileNotFoundException("Restaurant does not have a logo.");
            }

            // 3. Tải ảnh gốc từ Google Drive
            var originalBytes = await GetBytesFromUrlAsync(restaurant.LogoUrl, ct);

            byte[] finalBytes = originalBytes;
            if (width.HasValue || height.HasValue)
            {
                // 4. Xử lý ảnh (thay đổi kích thước) nếu có yêu cầu
                finalBytes = ResizeBytes(originalBytes, width, height);
            }

            var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(60));
            _cache.Set(cacheKey, finalBytes, cacheOptions);

            return finalBytes;
        }

        public async Task<byte[]> GetOrProcessMenuItemImageAsync(int menuItemId, int? w, int? h, CancellationToken ct)
        {
            string cacheKey = $"menu_item_{menuItemId}_w{w ?? 0}_h{h ?? 0}";

            if (_cache.TryGetValue(cacheKey, out byte[]? cached) && cached is not null)
                return cached;

            var imageUrl = await _uow.Repository<MenuItem>().GetQueryable()
                .Where(m => m.ItemId == menuItemId && !m.IsDeleted)
                .Select(m => m.ImageData)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new FileNotFoundException("Không tìm thấy ảnh hoặc món ăn.");

            var originalBytes = await GetBytesFromUrlAsync(imageUrl, ct);
            var resizedBytes = (w == null && h == null) ? originalBytes : ResizeBytes(originalBytes, w, h);

            var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1));
            _cache.Set(cacheKey, resizedBytes, cacheOptions);

            return resizedBytes;
        }


        #region Helpers
        private async Task<byte[]> GetBytesFromUrlAsync(string url, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL is empty", nameof(url));

            // Nếu là Google Drive link => trích id và chuyển thành uc?export=download
            var fileId = ExtractFileId(url);
            string fetchUrl = !string.IsNullOrWhiteSpace(fileId)
                ? $"https://drive.google.com/uc?export=download&id={fileId}"
                : url;

            var client = _httpFactory.CreateClient();
            using var resp = await client.GetAsync(fetchUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();
            var bytes = await resp.Content.ReadAsByteArrayAsync(ct);

            if (bytes == null || bytes.Length == 0)
                throw new FileNotFoundException("Không tải được nội dung ảnh từ URL.");

            return bytes;
        }

        private static string? ExtractFileId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            // dạng ?id=xxx
            var m1 = Regex.Match(url, @"[?&]id=([^&]+)");
            if (m1.Success) return m1.Groups[1].Value;

            // dạng /file/d/xxx/
            var m2 = Regex.Match(url, @"/file/d/([^/]+)");
            if (m2.Success) return m2.Groups[1].Value;

            // đôi khi người ta lưu dạng open?id=...
            var m3 = Regex.Match(url, @"open\?id=([^&]+)");
            if (m3.Success) return m3.Groups[1].Value;

            return null;
        }

        private static byte[] ResizeBytes(byte[] originalBytes, int? w, int? h)
        {
            using var image = Image.Load(originalBytes); // auto-detect format

            if (w == null && h == null)
            {
                return originalBytes;
            }

            int width = w ?? 0;
            int height = h ?? 0;

            // nếu một trong hai bằng 0 -> giữ tỉ lệ
            if (width == 0 && height > 0)
                width = (int)(image.Width * ((double)height / image.Height));
            else if (height == 0 && width > 0)
                height = (int)(image.Height * ((double)width / image.Width));
            else if (width == 0 && height == 0)
            {
                width = image.Width;
                height = image.Height;
            }

            var resizeOptions = new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max // giữ tỉ lệ, không cắt
            };

            image.Mutate(x => x.Resize(resizeOptions));

            using var ms = new MemoryStream();
            // Luôn lưu lại dưới dạng JPEG để đơn giản hóa (bạn có thể detect format và trả về tương ứng nếu cần)
            image.SaveAsJpeg(ms, new JpegEncoder { Quality = 85 });
            return ms.ToArray();
        }
        #endregion
    }
}
