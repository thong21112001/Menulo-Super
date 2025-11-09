using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Features.ResTables.Interfaces
{
    public interface IResTablesService
    {
        Task<ResTableDto> CreateAsync(CreateResTableDto dto, CancellationToken ct = default);
        Task<ResTableDto> UpdateAsync(UpdateResTableDto dto, CancellationToken ct = default);
        Task DeleteAsync(int tableId, CancellationToken ct = default);
        Task<ResTableResponse?> GetByIdAsync(int tableId, CancellationToken ct = default);
        IQueryable<RestaurantTable> GetQueryableRestaurantTableForCurrentUser();

        /// <summary>
        /// Lấy trạng thái (realtime) của TẤT CẢ các bàn
        /// thuộc về nhà hàng của người dùng hiện tại (Admin).
        /// </summary>
        Task<List<TableStatusDto>> GetTableStatusForCurrentUserAsync(CancellationToken ct = default);
    }
}
