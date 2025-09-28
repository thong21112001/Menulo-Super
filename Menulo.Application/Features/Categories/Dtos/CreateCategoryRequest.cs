using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Menulo.Application.Features.Categories.Dtos
{
    public sealed class CreateCategoryRequest
    {
        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Tên nhà hàng")]
        public int RestaurantId { get; set; }

        [Display(Name = "Độ ưu tiên hiển thị")]
        public int Priority { get; set; } = 1;
    }

    public sealed class UpdateCategoryRequest
    {
        public int CategoryId { get; set; }

        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Tên nhà hàng")]
        public int RestaurantId { get; set; }

        [Display(Name = "Độ ưu tiên hiển thị")]
        public int Priority { get; set; }
    }

    public sealed record CategoryResponse(
        int CategoryId, string CategoryName, int RestaurantId, int Priority,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        string? RestaurantName);
}
