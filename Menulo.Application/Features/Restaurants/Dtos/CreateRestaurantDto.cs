namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed record CreateRestaurantDto
    (
        string RestaurantName,
        string? RestaurantAddress,
        string? RestaurantPhone,
        byte[]? RestaurantLogo
    );
}
