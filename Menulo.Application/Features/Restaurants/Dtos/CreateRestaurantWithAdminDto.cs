namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed record CreateRestaurantWithAdminDto
    (
        // Thông tin nhà hàng
        string Name,
        string Address,
        string Phone,

        // Thông tin tài khoản Admin
        string FullName,
        string Username,
        string Email,
        string Password
    );
}
