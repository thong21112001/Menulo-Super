using AutoMapper;
using AutoMapper.QueryableExtensions;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Application.Features.ResTables.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Application.Features.ResTables.Services
{
    public class ResTablesService : IResTablesService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<RestaurantTable> _repo;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;


        public ResTablesService(IUnitOfWork uow, IMapper mapper, ICurrentUser currentUser)
        {
            _uow = uow;
            _repo = _uow.Repository<RestaurantTable>();
            _mapper = mapper;
            _currentUser = currentUser;
        }


        public async Task<ResTableDto> CreateAsync(CreateResTableDto dto, CancellationToken ct = default)
        {
            // Khóa tenant: non-superadmin chỉ được tạo cho chính nhà hàng của họ
            var restaurantId = _currentUser.IsSuperAdmin
                ? dto.RestaurantId // superadmin có thể chọn
                : _currentUser.RestaurantId ?? throw new UnauthorizedAccessException("Missing tenant");

            // Nếu bỏ trống hoặc <= 0 thì tự động tạo 1 bàn
            var quantity = dto.TableQuantity ?? 1;
            if (quantity <= 0) quantity = 1;

            // Đếm số lượng bàn hiện có của nhà hàng
            var repo = _uow.Repository<RestaurantTable>();
            var currentTableCount = await repo.GetQueryable()
                .CountAsync(t => t.RestaurantId == restaurantId, ct);

            var tablesToAdd = new List<RestaurantTable>();

            for (int i = 1; i <= quantity; i++)
            {
                tablesToAdd.Add(new RestaurantTable
                {
                    RestaurantId = restaurantId,
                    TableCode = Guid.NewGuid().ToString(),
                    // Nếu không nhập description thì tự động tạo, ngược lại dùng description đã nhập
                    Description = string.IsNullOrWhiteSpace(dto.Description)
                        ? $"Bàn số {currentTableCount + i}"
                        : (quantity == 1 ? dto.Description : $"{dto.Description} {i}")
                });

            }

            await _repo.AddRangeAsync(tablesToAdd, ct);
            await _uow.SaveChangesAsync(ct);

            // Trả về bàn đầu tiên vừa tạo
            return _mapper.Map<ResTableDto>(tablesToAdd.First());
        }

        public Task<ResTableDto> UpdateAsync(UpdateResTableDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteAsync(int tableId, CancellationToken ct = default)
        {
            var entity = await _repo.GetQueryable()
                        .FirstOrDefaultAsync(r => r.TableId == tableId, ct)
                         ?? throw new KeyNotFoundException("Table not found");

            // Ktra du lieu da phat sinh chua
            var orderRepo = _uow.Repository<Order>().GetQueryable();

            // ItemsTmp không có RestaurantId trực tiếp → kiểm qua bàn
            var tableRepo = _uow.Repository<RestaurantTable>().GetQueryable();
            var hasTmp = await tableRepo
                    .Where(t => t.TableId == tableId)
                    .AnyAsync(t => t.ItemsTmps.Any(), ct);

            var hasData = await orderRepo.AnyAsync(x => x.TableId == tableId, ct) || hasTmp;

            if (hasData)
                throw new InvalidOperationException("Bàn đã phát sinh dữ liệu nên không thể xoá.");

            // Cho xoá khi sạch dữ liệu
            await _repo.DeleteAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task<ResTableResponse?> GetByIdAsync(int tableId, CancellationToken ct = default)
        {
            var q = _repo.GetQueryable().AsNoTracking();

            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
                q = q.Where(c => c.RestaurantId == rid);

            var dto = await q
                .Where(c => c.TableId == tableId)
                .ProjectTo<ResTableResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(ct);

            if (dto is null) return null;

            if (!_currentUser.IsSuperAdmin)
            {
                dto = dto with { RestaurantName = null };
            }

            return dto;
        }

        public IQueryable<RestaurantTable> GetQueryableRestaurantTableForCurrentUser()
        {
            var query = _repo.GetQueryable()
                .Include(c => c.Restaurant)
                .AsNoTracking();

            // Áp dụng logic lọc theo quyền
            if (!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
                query = query.Where(c => c.RestaurantId == rid);

            return query;
        }
    }
}
