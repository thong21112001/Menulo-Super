using Menulo.Application.Features.Orders.Dtos;

namespace Menulo.Application.Features.Orders.Interfaces
{
    /// <summary>
    /// Thanh toán, Cập nhập số lượng, Khuyến mãi
    /// Chuyển từ đơn tạm thành đơn chính thức
    /// Lấy thông tin đơn hàng
    /// </summary>
    public interface IOrdersService
    {
        Task<OrderViewDto> ConvertFromTmpAsync(int restaurantId, int tableId, byte discount, string? customerPhone, CancellationToken ct = default);
        Task PayAsync(int orderId, CancellationToken ct = default);
        Task UpdateItemPriceAsync(int orderId, int itemId, decimal newPrice, CancellationToken ct = default);
        Task UpdateDiscountAsync(int orderId, byte discount, CancellationToken ct = default);
        Task<OrderViewDto?> GetAsync(int orderId, CancellationToken ct = default);
        Task AddItemsByStaffAsync(int tableId, AddOrderItemsListRequest req, string? staffUserId, CancellationToken ct = default);
    }
}
