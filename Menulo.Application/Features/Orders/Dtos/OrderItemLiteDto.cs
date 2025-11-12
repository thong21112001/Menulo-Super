namespace Menulo.Application.Features.Orders.Dtos
{
    public sealed class OrderItemLiteDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal LineTotal => Quantity * Price;
    }
}
