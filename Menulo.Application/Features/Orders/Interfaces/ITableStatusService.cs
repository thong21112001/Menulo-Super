using Menulo.Application.Features.Orders.Dtos;

namespace Menulo.Application.Features.Orders.Interfaces
{
    /// <summary>
    /// Đọc realtime status theo nhà hàng
    /// </summary>
    public interface ITableStatusService
    {
        Task<IReadOnlyList<TableLiveStatusDto>> GetLiveStatusAsync(int restaurantId, CancellationToken ct = default);
    }
}
