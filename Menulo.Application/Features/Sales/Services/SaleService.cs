using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Sales.Dtos;
using Menulo.Application.Features.Sales.Interfaces;

namespace Menulo.Application.Features.Sales.Services
{
    public class SaleService : ISaleService
    {
        private readonly IMapper _mapper;
        private readonly IIdentityService _identityService;


        public SaleService(IMapper mapper, IIdentityService identityService)
        {
            _mapper = mapper;
            _identityService = identityService;
        }

        public async Task<SaleDto> CreateAsync(CreateSaleDto dto, CancellationToken ct = default)
        {
            // 1. Gọi Abstraction (IIdentityService)
            var result = await _identityService.CreateUserAsync(
                dto.Username,
                dto.Password,
                dto.Email,
                dto.FullName,
                dto.PhoneNumber,
                "sale" // Hardcode role "sale" như yêu cầu
            );

            // 2. Kiểm tra kết quả
            if (!result.Succeeded)
            {
                // Nếu thất bại, ném ra một lỗi để Controller/PageModel có thể bắt
                throw new InvalidOperationException(
                    "Tạo tài khoản thất bại: " + string.Join(", ", result.Errors)
                );
            }

            // 3. Nếu thành công, trả về DTO
            var saleDto = new SaleDto(
                UserId: result.UserId,
                FullName: dto.FullName,
                Username: dto.Username,
                Email: dto.Email,
                PhoneNumber: dto.PhoneNumber
            );

            // 4. Trả về DTO đã được tạo thủ công (không dùng AutoMapper)
            return saleDto;
        }

        public async Task DeleteAsync(string userId, CancellationToken ct = default)
        {
            // 1. Phải 'await' để nhận về đối tượng 'IdentityResultDto'
            var result = await _identityService.DeleteUserAsync(userId, ct);

            // 2. Kiểm tra kết quả (GIỐNG HỆT NHƯ CREATEASYNC)
            if (!result.Succeeded)
            {
                // 3. Ném lỗi để Controller có thể bắt
                // Lỗi này sẽ được SalesController bắt và trả về 409 Conflict
                throw new InvalidOperationException(
                    "Xóa tài khoản thất bại: " + string.Join(", ", result.Errors)
                );
            }
        }

        public Task<SaleDto?> GetByIdAsync(string userId, CancellationToken ct = default)
        {
            return _identityService.GetUserByIdAsync(userId,ct);
        }

        public IQueryable<SaleRowDto> GetQueryableSales()
        {
            return _identityService.GetUsersAsQueryable("sale");
        }
    }
}
