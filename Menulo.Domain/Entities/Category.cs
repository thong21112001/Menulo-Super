using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Menulo.Domain.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        [DisplayName("Tên nhà hàng")]
        public int RestaurantId { get; set; }

        [DisplayName("Tên danh mục")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string CategoryName { get; set; } = null!;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public Restaurant? Restaurant { get; set; } = null!;
    }
}
