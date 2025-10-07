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

            return Page();
        }



        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values
                                              .SelectMany(modelStateEntry => modelStateEntry.Errors)
                                              .Select(error => error.ErrorMessage)
                                              .ToList();

                TempData.SetError("Lỗi: " + string.Join(" | ", errorMessages));
                return Page();
            }

            // Lấy entity hiện tại để có CurrentLogoUrl (nếu cần hiển thị lại khi lỗi)
            var current = await _svc.GetByIdAsync(Input.RestaurantId);
            if (current is null) return NotFound();
            CurrentLogoUrl = current.LogoUrl;

            // Nếu người dùng upload logo mới
            if (ImgUpload is { Length: > 0 })
            {
                var ext = Path.GetExtension(ImgUpload.FileName).ToLowerInvariant();
                if (!AllowedExts.Contains(ext))
                {
                    ModelState.AddModelError(nameof(ImgUpload), "Chỉ chấp nhận .jpg, .jpeg, .png.");
                    return Page();
                }

                // Transactional trong service:
                // Upload ảnh mới -> cập nhật DB -> commit -> xóa ảnh cũ (sau commit)
                await using var s = ImgUpload.OpenReadStream();
                await _svc.ReplaceLogoAsync(
                    restaurantId: Input.RestaurantId,
                    restaurantName: Input.Name,               // dùng cho slug folder Drive
                    newLogoStream: s,
                    newLogoFileName: ImgUpload.FileName,
                    contentType: ImgUpload.ContentType ?? "image/jpeg",
                    ct: ct
                );
            }

            // Cập nhật các trường text (Name/Address/Phone).
            // LogoUrl để null để service hiểu là "không đụng" (giữ nguyên).
            var dto = new UpdateRestaurantDto(
                RestaurantId: Input.RestaurantId,
                Name: Input.Name,
                Address: Input.Address,
                Phone: Input.Phone,
                LogoUrl: null
            );
            await _svc.UpdateAsync(dto, ct);

            TempData.SetSuccess("Cập nhật nhà hàng thành công!");
            return RedirectToPage("./Index");
        }
    }
}
