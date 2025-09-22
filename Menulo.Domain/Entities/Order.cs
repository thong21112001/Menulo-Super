namespace Menulo.Domain.Entities
{
    public class Order
    {
        public int OrderId { get; set; }

        public int RestaurantId { get; set; }

        public int TableId { get; set; }

        public DateTime? OrderTime { get; set; }

        public string Status { get; set; } = null!;

        public byte Discount { get; set; }

        public bool IsEInvoiceExported { get; set; } = false;

        public string? CustomerPhone { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public Restaurant Restaurant { get; set; } = null!;

        public RestaurantTable Table { get; set; } = null!;
    }
}
