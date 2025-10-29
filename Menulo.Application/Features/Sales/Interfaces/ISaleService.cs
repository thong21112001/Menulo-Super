using Menulo.Application.Features.Sales.Dtos;

namespace Menulo.Application.Features.Sales.Interfaces
{
    public interface ISaleService
    {
        Task<SaleDto> CreateAsync(CreateSaleDto dto, CancellationToken ct = default);
        //Task<SaleDto> UpdateAsync(UpdateResTableDto dto, CancellationToken ct = default);
        //Task DeleteAsync(int tableId, CancellationToken ct = default);
        //Task<ResTableResponse?> GetByIdAsync(int tableId, CancellationToken ct = default);
        //IQueryable<RestaurantTable> GetQueryableRestaurantTableForCurrentUser();
    }
}
