using System.ComponentModel.DataAnnotations;

namespace Menulo.Domain.Entities
{
    public class MenuItem
    {
        public int ItemId { get; set; }

        [Display(Name = "Danh sách nhà hàng")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int RestaurantId { get; set; }

        [Display(Name = "Danh mục món")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public int CategoryId { get; set; }

        [Display(Name = "Tên món")]
        [Required(ErrorMessage = "{0} không được để trống")]
        public string ItemName { get; set; } = null!;

        [Display(Name = "Thông tin món")]
        public string? Description { get; set; }

        [Display(Name = "Giá tiền")]
        [DisplayFormat(DataFormatString = "{0:G29}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageData { get; set; }

        public bool IsDeleted { get; set; }

        public Category? Category { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<ItemsTmp> ItemsTmps { get; set; } = new List<ItemsTmp>();

        public Restaurant? Restaurant { get; set; } = null!;
    }
}
