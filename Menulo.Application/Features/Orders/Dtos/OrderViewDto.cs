namespace Menulo.Application.Features.Orders.Dtos
{
    public sealed class OrderViewDto
    {
        public int OrderId { get; set; }
        public int RestaurantId { get; set; }
        public int TableId { get; set; }
        public DateTime? OrderTime { get; set; }
        public string Status { get; set; } = null!; // e.g. NEW, CONFIRMED, SERVING, PAID, CANCELLED
        public byte Discount { get; set; }
        public string? CustomerPhone { get; set; }
        public IReadOnlyList<OrderItemLiteDto> Items { get; set; } = Array.Empty<OrderItemLiteDto>();
        public decimal Subtotal => Items.Sum(x => x.LineTotal);
        public decimal DiscountAmount => Math.Round(Subtotal * Discount / 100m, 2);
        public decimal GrandTotal => Subtotal - DiscountAmount;
    }
}
