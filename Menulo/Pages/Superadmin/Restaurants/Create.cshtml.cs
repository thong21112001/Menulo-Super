using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Menulo.Pages.Superadmin.Restaurants
{
    public class CreateModel : PageModel
    {
        private readonly IRestaurantService _svc;

        [BindProperty]
        public RestaurantRequest.Create Input { get; set; } = new();

        [BindProperty]
        public IFormFile? ImgUpload { get; set; } // nhận file từ form


        public CreateModel(IRestaurantService svc)
        {
            _svc = svc;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid) return Page();

            if (ImgUpload is not { Length: > 0 })
            {
                ModelState.AddModelError(nameof(ImgUpload), "Vui lòng chọn file ảnh.");
                return Page();
            }

            // Validate định dạng & kích thước
            var ext = Path.GetExtension(ImgUpload.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(nameof(ImgUpload), "Chỉ chấp nhận .jpg, .jpeg, .png.");
                return Page();
            }
            const long MaxSize = 5L * 1024 * 1024; // 5MB
            if (ImgUpload.Length > MaxSize)
            {
                ModelState.AddModelError(nameof(ImgUpload), "Kích thước tối đa 5MB.");
                return Page();
            }

            try
            {
                await using var s = ImgUpload.OpenReadStream();
                var created = await _svc.CreateWithLogoAsync(
                    Input.Name, Input.Address, Input.Phone,
                    s, ImgUpload.FileName, ImgUpload.ContentType ?? "image/jpeg",
                    ct);

                TempData.SetSuccess("Tạo mới nhà hàng thành công!");
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData.SetError("Không thể tạo nhà hàng: " + ex.Message);
                return Page();
            }
        }
    }
}
