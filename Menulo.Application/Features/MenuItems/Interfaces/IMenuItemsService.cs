using Menulo.Application.Features.MenuItems.Dtos;

namespace Menulo.Application.Features.MenuItems.Interfaces
{
    public interface IMenuItemsService
    {
        /// <summary>
        /// Tạo một món ăn mới, xử lý việc tải ảnh lên
        /// </summary>
        Task<MenuItemDto> CreateMenuItemAsync(
            CreateMenuItemDto dto,
            Stream? imageStream,      // Service chỉ nhận Stream (sạch)
            string? imageFileName,    // Tên file gốc (để lấy extension)
            string? contentType,      // vd: "image/jpeg"
            CancellationToken ct = default
        );

        /// <summary>
        /// Lấy IQueryable các món ăn đã được lọc theo tenant (admin/superadmin)
        /// </summary>
        IQueryable<MenuItemRowDto> GetQueryableMenuItemsForCurrentUser();

        /// <summary>
        /// Đảo ngược trạng thái IsAvailable của món ăn
        /// </summary>
        /// <returns>Trạng thái IsAvailable mới</returns>
        Task<bool> ToggleMenuItemAvailabilityAsync(int menuItemId, CancellationToken ct = default);

        /// <summary>
        /// Lấy toàn bộ menu cho nhà hàng của user hiện tại,
        /// đã được lọc, sắp xếp và nhóm theo danh mục.
        /// </summary>
        Task<List<MenuCategoryGroupDto>> GetMenuForCurrentUserAsync(CancellationToken ct = default);
    }
}
