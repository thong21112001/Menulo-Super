namespace Menulo.Application.Features.Orders.Dtos
{
    public sealed class TransferDto
    {
        public int OrderId { get; set; }
        public int ToTableId { get; set; }
    }
}
