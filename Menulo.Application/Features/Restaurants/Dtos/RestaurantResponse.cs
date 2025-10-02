using System.Text.Json.Serialization;

namespace Menulo.Application.Features.Restaurants.Dtos
{
    // Dùng cho DataTable list
    public sealed record RestaurantRowDto(
        int RestaurantId,
        string Name,
        string? Address,
        string? Phone
    );

    // Dùng cho API xem chi tiết
    public sealed record RestaurantDetailsDto(
        int RestaurantId,
        string Name,
        string? Address,
        string? Phone,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? LogoUrl,

        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        DateTime? CreatedAt
    );
}
