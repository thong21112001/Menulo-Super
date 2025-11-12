using Menulo.Application.Features.Orders.Dtos;

namespace Menulo.Application.Features.Orders.Interfaces
{
    /// <summary>
    /// Giỏ tạm ItemsTmp đợi xác nhận thành đơn hàng
    /// </summary>
    public interface ITmpCartService
    {
        Task AddAsync(int restaurantId, int tableId, int itemId, int quantity, CancellationToken ct = default);
        Task RemoveAsync(int itemsTmpId, CancellationToken ct = default);
        Task RemoveAllAsync(int restaurantId, int tableId, CancellationToken ct = default);
        Task<IReadOnlyList<OrderItemLiteDto>> GetAsync(int restaurantId, int tableId, CancellationToken ct = default);
    }
}
