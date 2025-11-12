namespace Menulo.Application.Features.Orders.Dtos
{
    public sealed class AddTmpItemRequest
    {
        public int RestaurantId { get; set; }
        public int TableId { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
