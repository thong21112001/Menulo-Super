namespace Menulo.Application.Features.Restaurants.Dtos
{
    /// <summary>
    /// sealed = Không cho phép kế thừa.
    /// Lý do chính: Bảo vệ thiết kế, tăng hiệu năng, và đảm bảo tính toàn vẹn, 
    /// đặc biệt với các đối tượng dữ liệu như record và DTOs.
    /// </summary>
    public sealed record RestaurantDto
    (
        int RestaurantId,
        string Name,
        string? Address,
        string? Phone,
        byte[]? LogoImage
    );
}
