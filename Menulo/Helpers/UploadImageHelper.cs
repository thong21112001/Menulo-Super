using Microsoft.AspNetCore.Mvc.ModelBinding;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Menulo.Helpers
{
    public static class UploadImageHelper
    {
        private static readonly string[] AllowedExtensions = [".png", ".jpg", ".jpeg"];
        private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB
        private const int DefaultHeight = 200;

        /// <summary>
        /// Xử lý ảnh upload (resize, đổi encoder, trả về byte[]).
        /// Trả về null nếu không hợp lệ.
        /// </summary>
        public static async Task<byte[]?> ProcessUploadAsync(IFormFile file, ModelStateDictionary modelState)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!AllowedExtensions.Contains(ext))
            {
                modelState.AddModelError("ImgUpload", "Chỉ chấp nhận file .jpg, .jpeg, .png");
                return null;
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                modelState.AddModelError("ImgUpload", "Kích thước tối đa là 5Mb.");
                return null;
            }

            try
            {
                using var image = await Image.LoadAsync(file.OpenReadStream());

                int newWidth = (int)(image.Width * ((double)DefaultHeight / image.Height));
                image.Mutate(x => x.Resize(newWidth, DefaultHeight));

                IImageEncoder encoder = ext switch
                {
                    ".png" => new PngEncoder(),
                    ".jpg" or ".jpeg" => new JpegEncoder(),
                    _ => new PngEncoder()
                };

                using var ms = new MemoryStream();
                await image.SaveAsync(ms, encoder);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                modelState.AddModelError("ImgUpload", $"Lỗi xử lý ảnh: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sử dụng ảnh mặc định, resize, trả về byte[].
        /// </summary>
        public static async Task<byte[]?> ProcessDefaultAsync(string rootPath, string relativePath, ModelStateDictionary modelState)
        {
            var fullPath = Path.Combine(rootPath, relativePath);
            if (!File.Exists(fullPath))
            {
                modelState.AddModelError("ImgUpload", $"File ảnh mặc định không tìm thấy: {relativePath}");
                return null;
            }

            try
            {
                var bytes = await File.ReadAllBytesAsync(fullPath);
                using var ms = new MemoryStream(bytes);
                using var image = await Image.LoadAsync(ms);

                int newWidth = (int)(image.Width * ((double)DefaultHeight / image.Height));
                image.Mutate(x => x.Resize(newWidth, DefaultHeight));

                using var resizedStream = new MemoryStream();
                await image.SaveAsync(resizedStream, new PngEncoder());
                return resizedStream.ToArray();
            }
            catch (Exception ex)
            {
                modelState.AddModelError("ImgUpload", $"Lỗi xử lý ảnh mặc định: {ex.Message}");
                return null;
            }
        }
    }
}
