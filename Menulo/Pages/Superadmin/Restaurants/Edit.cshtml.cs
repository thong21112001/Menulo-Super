using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Menulo.Pages.Superadmin.Restaurants
{
    public class EditModel : PageModel
    {
        private readonly IRestaurantService _svc;
        private static readonly string[] AllowedExts = [".jpg", ".jpeg", ".png"];

        [BindProperty]
        public RestaurantRequest.Update Input { get; set; } = new();

        [BindProperty]
        public IFormFile? ImgUpload { get; set; }

        // Chỉ để hiển thị logo hiện tại
        public string? CurrentLogoUrl { get; private set; }
        public long? CurrentLogoVersion { get; private set; } // Dùng long để an toàn


        public EditModel(IRestaurantService svc)
        {
            _svc = svc;
        }



        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurantDb = await _svc.GetByIdAsync(id.Value);

            if (restaurantDb == null)
                return NotFound();

            Input = new RestaurantRequest.Update
            {
                RestaurantId = restaurantDb.RestaurantId,
                Name = restaurantDb.Name,
                Address = restaurantDb.Address,
                Phone = restaurantDb.Phone,
            };

            CurrentLogoUrl = restaurantDb.LogoUrl;
            // Lấy Ticks để làm version, nếu null thì thôi
            CurrentLogoVersion = restaurantDb.LogoUpdatedAtUtc?.Ticks;

            return Page();
        }



        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            // Lấy URL logo hiện tại để hiển thị lại nếu có lỗi validation
            var current = await _svc.GetByIdAsync(Input.RestaurantId);
            if (current is null) return NotFound();
            CurrentLogoUrl = current.LogoUrl;
            CurrentLogoVersion = current.LogoUpdatedAtUtc?.Ticks;

            // Kiểm tra validation cho file upload
            if (ImgUpload is { Length: > 0 })
            {
                var ext = Path.GetExtension(ImgUpload.FileName).ToLowerInvariant();
                if (!AllowedExts.Contains(ext))
                {
                    ModelState.AddModelError(nameof(ImgUpload), "Chỉ chấp nhận .jpg, .jpeg, .png.");
                }
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Chuẩn bị DTO với các trường text
            var dto = new UpdateRestaurantDto(
                RestaurantId: Input.RestaurantId,
                Name: Input.Name,
                Address: Input.Address,
                Phone: Input.Phone,
                LogoUrl: null // LogoUrl không dùng ở đây, sẽ được xử lý trong service
            );

            // Mở stream và gọi service (stream sẽ là null nếu không có file upload)
            await using var stream = ImgUpload?.OpenReadStream();

            await _svc.UpdateWithOptionalLogoAsync(
                dto,
                stream,
                ImgUpload?.FileName,
                ImgUpload?.ContentType,
                ct
            );

            TempData.SetSuccess("Cập nhật nhà hàng thành công!");
            return RedirectToPage("./Index");
        }
    }
}
