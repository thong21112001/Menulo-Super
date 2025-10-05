using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Extensions;
using Menulo.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Menulo.Pages.Superadmin.Restaurants
{
    public class EditModel : PageModel
    {
        private readonly IRestaurantService _svc;
        private readonly IWebHostEnvironment _environment;

        [BindProperty]
        public RestaurantRequest.Update Input { get; set; } = new();

        [BindProperty]
        public IFormFile? ImgUpload { get; set; }


        public EditModel(IRestaurantService svc, IWebHostEnvironment environment)
        {
            _svc = svc;
            _environment = environment;
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
                LogoImage = restaurantDb.LogoUrl
            };

            return Page();
        }



        public async Task<IActionResult> OnPostAsync()
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

            byte[]? imageData = null;

            if (ImgUpload != null && ImgUpload.Length > 0)
            {
                // Trường hợp 1: có upload mới
                imageData = await UploadImageHelper.ProcessUploadAsync(ImgUpload, ModelState);
                if (imageData == null) return Page();
            }
            else
            {
                // giữ nguyên logo cũ
                imageData = null;
            }

            // Map Request -> DTO
            var dto = new UpdateRestaurantDto(
                RestaurantId: Input.RestaurantId,
                Name: Input.Name,
                Address: Input.Address,
                Phone: Input.Phone,
                LogoImage: imageData
            );

            await _svc.UpdateAsync(dto);
            TempData.SetSuccess("Cập nhật nhà hàng thành công!");
            return RedirectToPage("./Index");
        }
    }
}
