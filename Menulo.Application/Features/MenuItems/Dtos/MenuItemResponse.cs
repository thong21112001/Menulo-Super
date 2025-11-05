namespace Menulo.Application.Features.MenuItems.Dtos
{
    public sealed record MenuItemRowDto
    {
        public MenuItemRowDto() { } // Cần cho AutoMapper.ProjectTo

        public int ItemId { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal? Price { get; init; }
        public bool IsAvailable { get; init; }
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public int RestaurantId { get; init; }
        public string? RestaurantName { get; init; } // Nullable cho Admin
    }
}
