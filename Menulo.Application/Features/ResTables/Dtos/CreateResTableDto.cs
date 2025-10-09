namespace Menulo.Application.Features.ResTables.Dtos
{
    public sealed record CreateResTableDto
    (
        int RestaurantId,
        string TableCode,
        string? Description
    );
}
