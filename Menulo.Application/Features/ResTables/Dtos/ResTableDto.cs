namespace Menulo.Application.Features.ResTables.Dtos
{
    public sealed record ResTableDto
    (
        int TableId,
        int RestaurantId,
        string TableCode,
        string? Description
    );
}
