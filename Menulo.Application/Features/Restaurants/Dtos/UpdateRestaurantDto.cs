namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed record UpdateRestaurantDto
    (
        int RestaurantId,
        string Name,
        string? Address,
        string? Phone,
        byte[]? LogoImage
    );
}
