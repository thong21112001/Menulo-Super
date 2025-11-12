namespace Menulo.Application.Features.Orders.Dtos
{
    public class ConvertOrderRequest
    {
        public int RestaurantId { get; set; }
        public int TableId { get; set; }
        public byte Discount { get; set; } = 0; // 0-100, tùy chính sách
        public string? CustomerPhone { get; set; }
    }
}
