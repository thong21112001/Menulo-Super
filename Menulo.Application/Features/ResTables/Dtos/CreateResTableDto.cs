namespace Menulo.Application.Features.ResTables.Dtos
{
    public sealed record CreateResTableDto
    (
        int RestaurantId,
        int? TableQuantity,
        string? Description
    );
}
