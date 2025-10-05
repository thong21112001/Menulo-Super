using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Extensions;
using Menulo.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;

namespace Menulo.Pages.Superadmin.Restaurants
{
    public class CreateModel : PageModel
    {
        private readonly IRestaurantService _svc;
        private readonly IWebHostEnvironment _environment;

        [BindProperty]
        public RestaurantRequest.Create Input { get; set; } = new();

        [BindProperty]
        public IFormFile? ImgUpload { get; set; } // nhận file từ form


        public CreateModel(IRestaurantService svc, IWebHostEnvironment environment)
        {
            _svc = svc;
            _environment = environment;
        }

        public IActionResult OnGet()
        {
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

            var imageData = ImgUpload != null
                ? await UploadImageHelper.ProcessUploadAsync(ImgUpload, ModelState)
                : await UploadImageHelper.ProcessDefaultAsync(_environment.WebRootPath, Path.Combine("images", "image.png"), ModelState);

            if (imageData == null)
            {
                return Page();
            }

            // Map Request -> DTO (đúng type service cần)
            var dto = new CreateRestaurantDto(
                Name: Input.Name,
                Address: Input.Address,
                Phone: Input.Phone,
                LogoImage: imageData
            );

            await _svc.CreateAsync(dto);
            TempData.SetSuccess("Tạo mới nhà hàng thành công!");
            return RedirectToPage("./Index");
        }
    }
}
