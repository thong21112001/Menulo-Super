namespace Menulo.Application.Features.Orders.Dtos
{
    /// <summary>
    /// Dùng để nhân viên thêm nhiều món vào giỏ tạm thời cùng lúc
    /// </summary>
    public sealed class AddOrderItemsListRequest
    {
        public List<ItemReq> Items { get; set; } = new();
        public sealed class ItemReq
        {
            public int ItemId { get; set; }
            public int Quantity { get; set; } = 1;
        }
    }
}
