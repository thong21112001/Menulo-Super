using Menulo.Application.Features.Sales.Dtos;

namespace Menulo.Application.Common.Interfaces
{
    // Đây là Abstraction (Tính trừu tượng)
    public interface IIdentityService
    {
        Task<IdentityResultDto> CreateUserAsync(
            string userName,
            string password,
            string email,
            string fullName,
            string phoneNumber,
            string role); // Thêm tham số role

        /// <summary>
        /// Kiểm tra xem Username đã được sử dụng hay chưa.
        /// </summary>
        Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default);

        /// <summary>
        /// Kiểm tra xem Email đã được sử dụng hay chưa.
        /// </summary>
        Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default);

        /// <summary>
        /// Kiểm tra xem Số điện thoại đã được sử dụng hay chưa.
        /// </summary>
        Task<bool> IsPhoneTakenAsync(string phoneNumber, CancellationToken ct = default);

        /// <summary>
        /// (Dùng cho DataTables) Trả về IQueryable đã được chiếu sang DTO.
        /// </summary>
        IQueryable<SaleRowDto> GetUsersAsQueryable(string roleName);

        /// <summary>
        /// (Dùng cho API Get Details) Lấy chi tiết user bằng ID.
        /// </summary>
        Task<SaleDto?> GetUserByIdAsync(string userId, CancellationToken ct);

        /// <summary>
        /// (Dùng cho API Delete) Xóa user bằng ID.
        /// </summary>
        Task<IdentityResultDto> DeleteUserAsync(string userId, CancellationToken ct);

        // ===== LIÊN KẾT USER VỚI NHÀ HÀNG =====
        Task<IdentityResultDto> SetUserRestaurantIdAsync(
            string userId,
            int restaurantId,
            CancellationToken ct = default);
    }
}
