namespace Menulo.Application.Features.Categories.Dtos
{
    public sealed record CreateCategoryDto(
        string CategoryName,
        int RestaurantId
    );
}
