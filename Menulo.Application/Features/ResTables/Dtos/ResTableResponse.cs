using System.Text.Json.Serialization;

namespace Menulo.Application.Features.ResTables.Dtos
{
    public sealed record ResTableResponse
    (
        int TableId,
        int RestaurantId,
        string? TableCode,
        string? Description,

        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
         string? RestaurantName
    );
}
