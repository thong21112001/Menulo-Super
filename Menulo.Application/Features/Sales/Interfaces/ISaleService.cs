using Menulo.Application.Features.Sales.Dtos;

namespace Menulo.Application.Features.Sales.Interfaces
{
    public interface ISaleService
    {
        Task<SaleDto> CreateAsync(CreateSaleDto dto, CancellationToken ct = default);
        //Task<SaleDto> UpdateAsync(UpdateResTableDto dto, CancellationToken ct = default);
        Task DeleteAsync(string userId, CancellationToken ct = default);
        Task<SaleDto?> GetByIdAsync(string userId, CancellationToken ct = default);
        IQueryable<SaleRowDto> GetQueryableSales();
    }
}
