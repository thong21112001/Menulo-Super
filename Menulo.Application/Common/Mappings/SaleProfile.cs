using AutoMapper;
using Menulo.Application.Features.Sales.Dtos;

namespace Menulo.Application.Common.Mappings
{
    public sealed class SaleProfile : Profile
    {
        public SaleProfile()
        {
            // ===== Request -> Command DTO =====
            // Tự động map từ SaleRequest.Create -> CreateSaleDto
            // AutoMapper sẽ tự động khớp các thuộc tính có cùng tên
            CreateMap<SaleRequest.Create, CreateSaleDto>();

            CreateMap<SaleRowDto, SaleRowDto>();
        }
    }
}
