namespace Menulo.Application.Features.MenuItems.Dtos
{
    public sealed record MenuItemDto
    (
        int ItemId,
        string ItemName,
        string? Description,
        decimal? Price,
        string? ImageData,
        bool IsAvailable,
        int CategoryId,
        string CategoryName,
        int RestaurantId
    );
}
