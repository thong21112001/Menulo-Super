namespace Menulo.Domain.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ItemId { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public MenuItem Item { get; set; } = null!;

        public Order Order { get; set; } = null!;
    }
}
