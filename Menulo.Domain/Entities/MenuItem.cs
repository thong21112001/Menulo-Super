namespace Menulo.Domain.Entities
{
    public class MenuItem
    {
        public int ItemId { get; set; }

        public int RestaurantId { get; set; }

        public int CategoryId { get; set; }

        public string ItemName { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageData { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsAvailable { get; set; } = true;   // Ẩn hoặc hiển thị món

        public Category? Category { get; set; } = null!;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public ICollection<ItemsTmp> ItemsTmps { get; set; } = new List<ItemsTmp>();

        public Restaurant? Restaurant { get; set; } = null!;
    }
}
