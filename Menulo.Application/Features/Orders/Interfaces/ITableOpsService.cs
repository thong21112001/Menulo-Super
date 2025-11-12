using Menulo.Application.Features.Orders.Dtos;

namespace Menulo.Application.Features.Orders.Interfaces
{
    /// <summary>
    /// Tách và gộp bàn (Bỏ vào chứ chưa dùng)
    /// </summary>
    public interface ITableOpsService
    {
        Task<IEnumerable<TableMiniDto>> GetAvailableAsync(int restaurantId, int currentTableId, CancellationToken ct = default);
        Task<IEnumerable<TableMiniDto>> GetActiveAsync(int restaurantId, int currentTableId, CancellationToken ct = default);
        Task TransferAsync(int orderId, int toTableId, CancellationToken ct = default);
        Task MergeAsync(int fromTableId, int toTableId, CancellationToken ct = default);
    }
}
