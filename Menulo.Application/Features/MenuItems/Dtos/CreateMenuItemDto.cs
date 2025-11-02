namespace Menulo.Application.Features.MenuItems.Dtos
{
    public sealed record CreateMenuItemDto
    (
        int CategoryId,
        int RestaurantId,
        string ItemName,
        decimal? Price,
        string? Description
    );
}
