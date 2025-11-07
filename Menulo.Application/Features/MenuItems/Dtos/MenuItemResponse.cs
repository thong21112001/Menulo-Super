using System.Text.Json.Serialization;

namespace Menulo.Application.Features.MenuItems.Dtos
{
    public sealed record MenuItemRowDto
    {
        public MenuItemRowDto() { } // Cần cho AutoMapper.ProjectTo

        public int ItemId { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal? Price { get; init; }
        public bool IsAvailable { get; init; }
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public int RestaurantId { get; init; }
        public string? RestaurantName { get; init; } // Nullable cho Admin
    }

    // Dùng cho API xem chi tiết
    public sealed record MenuItemDetailsDto {
        public MenuItemDetailsDto() { }

        public int ItemId { get; init; }
        public string ItemName { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal? Price { get; init; }
        public int CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public string? ImageData { get; init; }
    }
}
