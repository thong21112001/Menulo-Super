namespace Menulo.Application.Features.Categories.Dtos
{
    public sealed record UpdateCategoryDto(
        int CategoryId,
        string CategoryName,
        int RestaurantId
    );
}
