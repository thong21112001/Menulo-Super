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
    }
}
