namespace Menulo.Application.Features.Orders.Dtos
{
    public sealed class TableLiveStatusDto
    {
        public int TableId { get; set; }
        public string Description { get; set; } = "";
        public bool HasOrder { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public bool HasPendingOrder { get; set; }
        public int PendingTotalQuantity { get; set; }
    }
}
