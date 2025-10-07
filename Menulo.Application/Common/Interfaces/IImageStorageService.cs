namespace Menulo.Application.Common.Interfaces
{
    public interface IImageStorageService
    {
        /// Upload ảnh lên kho (Drive, S3, local, …) và trả về URL công khai
        Task<string> UploadAsync(Stream content, string fileName, string contentType, int restaurantId, string restaurantName, string logicalName);

        /// Upload ảnh mới và xoá ảnh cũ (nếu có) theo URL công khai
        Task<string> UploadAndReplaceAsync(Stream content, string fileName, string contentType, int restaurantId, string restaurantName, string logicalName, string? oldPublicUrl);

        /// Xoá ảnh theo URL công khai
        Task DeleteByPublicUrlAsync(string publicUrl);
    }
}
