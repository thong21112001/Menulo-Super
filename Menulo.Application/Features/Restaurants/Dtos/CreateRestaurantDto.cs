namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed record CreateRestaurantDto
    (
        string Name,
        string? Address,
        string? Phone,
        string? LogoUrl
    );
}
