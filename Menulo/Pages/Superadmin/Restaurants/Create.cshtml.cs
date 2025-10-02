using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

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

            var imageData = await ProcessImageToByteArrayAsync();

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


        #region Viết phương thức xử lý
        /// <summary>
        /// Xử lý ảnh upload hoặc ảnh mặc định, resize và trả về dữ liệu dạng byte array.
        /// Trả về null nếu có lỗi validation (và đã cập nhật ModelState).
        /// </summary>
        /// <returns>Một mảng byte của ảnh đã được resize, hoặc null nếu có lỗi.</returns>
        private async Task<byte[]?> ProcessImageToByteArrayAsync()
        {
            // Trường hợp 1: Người dùng có upload file mới
            if (ImgUpload != null && ImgUpload.Length > 0)
            {
                try
                {
                    // === Validation file upload ===
                    var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
                    var fileExtension = Path.GetExtension(ImgUpload.FileName).ToLower();

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ModelState.AddModelError("ImgUpload", "Chỉ chấp nhận file .jpg, .jpeg, .png");
                        return null; // Trả về null khi validation thất bại
                    }

                    var maxFileSizeInBytes = 5 * 1024 * 1024; // 5MB
                    if (ImgUpload.Length > maxFileSizeInBytes)
                    {
                        ModelState.AddModelError("ImgUpload", "Kích thước tối đa là 5Mb.");
                        return null;
                    }

                    // === Xử lý ảnh ===
                    using (Image image = await Image.LoadAsync(ImgUpload.OpenReadStream()))
                    {
                        // Resize ảnh
                        int newHeight = 200;
                        int newWidth = (int)(image.Width * ((double)newHeight / image.Height));
                        image.Mutate(x => x.Resize(newWidth, newHeight));

                        // Chọn định dạng encoder dựa trên đuôi file
                        IImageEncoder encoder = fileExtension switch
                        {
                            ".png" => new PngEncoder(),
                            ".jpg" or ".jpeg" => new JpegEncoder(),
                            _ => new PngEncoder() // Mặc định là PNG
                        };

                        // Lưu ảnh đã resize vào một MemoryStream
                        using var resizedStream = new MemoryStream();
                        await image.SaveAsync(resizedStream, encoder);

                        // THAY ĐỔI QUAN TRỌNG: Trả về mảng byte thay vì upload
                        return resizedStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImgUpload", $"Lỗi xử lý ảnh upload: {ex.Message}");
                    return null;
                }
            }

            // Trường hợp 2: Người dùng không upload, dùng ảnh mặc định
            else
            {
                try
                {
                    // Tìm ảnh mặc định trong wwwroot
                    var defaultRelativePath = Path.Combine("images", "image.png");
                    var defaultFullPath = Path.Combine(_environment.WebRootPath, defaultRelativePath);

                    if (!System.IO.File.Exists(defaultFullPath))
                    {
                        ModelState.AddModelError("ImgUpload", $"File ảnh mặc định không tìm thấy: {defaultRelativePath}");
                        return null;
                    }

                    // Đọc file ảnh mặc định
                    byte[] defaultImageBytes = await System.IO.File.ReadAllBytesAsync(defaultFullPath);

                    using (var ms = new MemoryStream(defaultImageBytes))
                    using (Image image = await Image.LoadAsync(ms))
                    {
                        // Resize ảnh
                        int newHeight = 200;
                        int newWidth = (int)(image.Width * ((double)newHeight / image.Height));
                        image.Mutate(x => x.Resize(newWidth, newHeight));

                        // Lưu ảnh đã resize vào một MemoryStream mới
                        using var resizedStream = new MemoryStream();
                        await image.SaveAsync(resizedStream, new PngEncoder()); // Ảnh mặc định là PNG

                        // THAY ĐỔI QUAN TRỌNG: Trả về mảng byte
                        return resizedStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ImgUpload", $"Lỗi khi xử lý ảnh mặc định: {ex.Message}");
                    return null;
                }
            }
        }
        #endregion
    }
}
