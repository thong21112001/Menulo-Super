namespace Menulo.WebAPI.Api.Contracts.Categories
{
    public sealed class CreateCategoryRequest
    {
        public string CategoryName { get; set; } = null!;
        public int RestaurantId { get; set; }
    }

    public sealed class UpdateCategoryRequest
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public int RestaurantId { get; set; }
    }

    public sealed record CategoryResponse(
        int CategoryId, string CategoryName, int RestaurantId, string? RestaurantName);
}
