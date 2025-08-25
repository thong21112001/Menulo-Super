using System.ComponentModel.DataAnnotations;

namespace Menulo.Domain.Entities
{
    public class RestaurantTable
    {
        public int TableId { get; set; }

        [Display(Name = "Nhà hàng")]
        public int RestaurantId { get; set; }

        [Display(Name = "Mã bàn")]
        public string TableCode { get; set; } = null!;

        [Display(Name = "Thông tin bàn")]
        public string? Description { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<ItemsTmp> ItemsTmps { get; set; } = new List<ItemsTmp>();

        public Restaurant? Restaurant { get; set; } = null!;
    }
}
