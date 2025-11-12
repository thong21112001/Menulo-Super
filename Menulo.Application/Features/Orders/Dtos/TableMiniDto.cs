namespace Menulo.Application.Features.Orders.Dtos
{
    public sealed class TableMiniDto
    {
        public int TableId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
