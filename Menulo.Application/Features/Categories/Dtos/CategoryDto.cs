namespace Menulo.Application.Features.Categories.Dtos
{
    /// <summary>
    /// sealed = Không cho phép kế thừa.
    /// Lý do chính: Bảo vệ thiết kế, tăng hiệu năng, và đảm bảo tính toàn vẹn, 
    /// đặc biệt với các đối tượng dữ liệu như record và DTOs.
    /// </summary>
    public sealed record CategoryDto(
        int CategoryId,
        string CategoryName,
        int RestaurantId,
        string? RestaurantName
    );
}
