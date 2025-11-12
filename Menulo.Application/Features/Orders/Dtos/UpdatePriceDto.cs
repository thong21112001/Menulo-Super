namespace Menulo.Application.Features.Orders.Dtos
{
    public sealed class UpdatePriceDto
    {
        public int ItemId { get; set; }
        public decimal NewPrice { get; set; }
    }
}
