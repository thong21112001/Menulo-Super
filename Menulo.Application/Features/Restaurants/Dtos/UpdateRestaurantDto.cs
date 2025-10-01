namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed record UpdateRestaurantDto
    (
        int RestaurantId,
        string RestaurantName,
        string? RestaurantAddress,
        string? RestaurantPhone,
        byte[]? RestaurantLogo
    );
}
