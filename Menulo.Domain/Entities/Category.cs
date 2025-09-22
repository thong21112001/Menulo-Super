namespace Menulo.Domain.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        public int RestaurantId { get; set; }

        public string CategoryName { get; set; } = null!;

        public int Priority { get; set; } = 1;  //Giá trị ưu tiên hiển thị, càng nhỏ càng ưu tiên

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public Restaurant? Restaurant { get; set; } = null!;
    }
}
