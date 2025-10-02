namespace Menulo.Application.Features.Restaurants.Dtos
{
    public sealed record CreateRestaurantDto
    (
        string Name,
        string? Address,
        string? Phone,
        byte[]? LogoImage
    );
}
