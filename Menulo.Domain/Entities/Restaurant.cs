using System.ComponentModel.DataAnnotations;

namespace Menulo.Domain.Entities
{
    public class Restaurant
    {
        public int RestaurantId { get; set; }

        [Display(Name = "Tên nhà hàng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string Name { get; set; } = null!;

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Display(Name = "Logo nhà hàng")]
        public byte[]? LogoImage { get; set; }

        public DateTime? CreatedAt { get; set; }

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<RestaurantTable> RestaurantTables { get; set; } = new List<RestaurantTable>();

        public ICollection<RestaurantAdmin> Admins { get; set; } = new List<RestaurantAdmin>();
    }
}
