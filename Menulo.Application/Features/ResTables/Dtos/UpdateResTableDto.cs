namespace Menulo.Application.Features.ResTables.Dtos
{
    public sealed record UpdateResTableDto
    (
        int TableId,
        int RestaurantId,
        string TableCode,
        string? Description
    );
}
