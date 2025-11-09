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

        public async Task<ResTableDto> UpdateAsync(UpdateResTableDto dto, CancellationToken ct = default)
        {
            var q = _repo.GetQueryable();

            if(!_currentUser.IsSuperAdmin && _currentUser.RestaurantId is int rid)
                q = q.Where(c => c.RestaurantId == rid);

            var entity = await q.FirstOrDefaultAsync(c => c.TableId == dto.TableId, ct)
                         ?? throw new KeyNotFoundException("Table not found");

            entity.Description = dto.Description?.Trim();

            if (_currentUser.IsSuperAdmin)
            {
                entity.RestaurantId = dto.RestaurantId;
            }

            await _repo.UpdateAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            var result = await _repo.GetQueryable()
                .AsNoTracking()
                .Where(c => c.TableId == entity.TableId)
                .ProjectTo<ResTableDto>(_mapper.ConfigurationProvider)
                .FirstAsync(ct);

            return result;
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

        public async Task<List<TableStatusDto>> GetTableStatusForCurrentUserAsync(CancellationToken ct = default)
        {
            // 1. Lấy RestaurantId của Admin (Bảo mật)
            var restaurantId = _currentUser.RestaurantId
                ?? throw new UnauthorizedAccessException("Người dùng không được gán cho nhà hàng nào.");

            // 2. Query dữ liệu từ DB với Include Orders và ItemsTmps
            var tables = await _repo.GetQueryable()
                .Where(t => t.RestaurantId == restaurantId)
                .Include(t => t.Orders.Where(o => o.Status == "Pending")) // Chỉ Include đơn "Pending"
                    .ThenInclude(o => o.OrderItems)
                .Include(t => t.ItemsTmps) // Đơn tạm
                .AsNoTracking()
                .ToListAsync(ct);

            // 3. Map (ánh xạ) sang DTO
            return tables.Select(t =>
            {
                // Lấy đơn hàng "Pending" (chỉ có 1 hoặc 0 vì đã lọc ở Include)
                var pendingOrder = t.Orders.FirstOrDefault(o => o.Status == "Pending");

                // Tính toán
                decimal totalBeforeDiscount = pendingOrder?.OrderItems.Sum(oi => oi.Quantity * oi.Price) ?? 0;
                byte discount = pendingOrder?.Discount ?? 0;
                decimal totalAmount = totalBeforeDiscount * (1 - (decimal)discount / 100);

                return new TableStatusDto(
                    TableId: t.TableId,
                    Description: t.Description ?? "Unknown",
                    HasOrder: pendingOrder != null,
                    TotalQuantity: pendingOrder?.OrderItems.Sum(oi => oi.Quantity) ?? 0,
                    TotalAmount: totalAmount,
                    HasPendingOrder: t.ItemsTmps.Any(),
                    PendingTotalQuantity: t.ItemsTmps.Sum(i => i.Quantity)
                );
            })
            .OrderByDescending(t => t.HasOrder || t.HasPendingOrder)
            .ToList();
        }
    }
}
