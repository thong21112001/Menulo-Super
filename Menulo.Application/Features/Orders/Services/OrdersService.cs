using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Orders.Dtos;
using Menulo.Application.Features.Orders.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Menulo.Application.Features.Orders.Services
{
    public sealed class OrdersService : IOrdersService
    {
        private readonly IUnitOfWork _uow;
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _tableLocks = new();
        private static SemaphoreSlim GetLock(int tableId)
        => _tableLocks.GetOrAdd(tableId, _ => new SemaphoreSlim(1, 1));

        public OrdersService(IUnitOfWork uow)
        {
            _uow = uow;
        }



        public Task<OrderViewDto> ConvertFromTmpAsync(int restaurantId, int tableId, byte discount, string? customerPhone, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<OrderViewDto?> GetAsync(int orderId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task PayAsync(int orderId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDiscountAsync(int orderId, byte discount, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateItemPriceAsync(int orderId, int itemId, decimal newPrice, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Thêm món vào order của bàn do nhân viên thêm // Có thể tạo mới order nếu chưa có
        /// </summary>
        public async Task AddItemsByStaffAsync(int tableId, AddOrderItemsListRequest req, string? staffUserId, CancellationToken ct = default)
        {
            if (req?.Items == null || !req.Items.Any())
                throw new InvalidOperationException("Không có món để thêm.");

            // lấy Table -> cần RestaurantId
            var tb = await _uow.Repository<RestaurantTable>().GetQueryable()
                        .FirstOrDefaultAsync(r => r.TableId == tableId, ct)
                     ?? throw new InvalidOperationException("Table not found");

            var locker = GetLock(tableId);
            await locker.WaitAsync(ct);
            try
            {
                await _uow.BeginTransactionAsync(ct);

                // tìm order Pending
                var ordRepo = _uow.Repository<Order>();
                var order = await ordRepo.GetQueryable()
                    .FirstOrDefaultAsync(o => o.TableId == tableId && o.Status == "Pending", ct);

                if (order != null && order.IsEInvoiceExported)
                {
                    await _uow.RollbackTransactionAsync(ct);
                    throw new InvalidOperationException("Hóa đơn đã được xuất cho bàn này, không thể thêm món mới.");
                }

                if (order == null)
                {
                    order = new Order
                    {
                        RestaurantId = tb.RestaurantId,
                        TableId = tableId,
                        OrderTime = DateTime.UtcNow,
                        Status = "Pending",
                    };
                    await ordRepo.AddAsync(order, ct);
                    await _uow.SaveChangesAsync(ct); // để có OrderId
                }

                // round mới (staff)
                var lastNo = await _uow.Repository<OrderHistory>().GetQueryable()
                    .Where(r => r.OrderId == order.OrderId)
                    .MaxAsync(r => (int?)r.RoundNo, ct) ?? 0;

                var round = new OrderHistory
                {
                    OrderId = order.OrderId,
                    RoundNo = lastNo + 1,
                    Source = "staff",
                    CreatedAt = DateTime.UtcNow,
                    CreatedByUserId = staffUserId
                };
                await _uow.Repository<OrderHistory>().AddAsync(round, ct);
                await _uow.SaveChangesAsync(ct); // để có OrderHistoryId

                // build set id
                var reqIds = req.Items.Select(i => i.ItemId).Where(i => i > 0).Distinct().ToHashSet();

                // load toàn bộ menu của nhà hàng (2 cột) rồi lọc trên memory
                var menuListAll = await _uow.Repository<MenuItem>().GetQueryable()
                    .Where(mi => mi.RestaurantId == tb.RestaurantId)
                    .Select(mi => new { mi.ItemId, mi.Price })
                    .ToListAsync(ct);

                var menuMap = menuListAll
                    .Where(mi => reqIds.Contains(mi.ItemId))
                    .ToDictionary(mi => mi.ItemId, mi => mi.Price);

                var invalids = reqIds.Where(id => !menuMap.ContainsKey(id)).ToList();
                if (invalids.Any())
                {
                    await _uow.RollbackTransactionAsync(ct);
                    throw new InvalidOperationException($"Các ItemId không thuộc nhà hàng: {string.Join(',', invalids)}");
                }

                // gom quantity nếu trùng
                var reqMap = req.Items
                    .Where(x => x.Quantity > 0)
                    .GroupBy(x => x.ItemId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                // insert
                var oiRepo = _uow.Repository<OrderItem>();
                foreach (var kv in reqMap)
                {
                    var itemId = kv.Key;
                    var qty = kv.Value;
                    var price = menuMap[itemId];

                    await oiRepo.AddAsync(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ItemId = itemId,
                        Quantity = qty,
                        Price = price,
                        OrderHistoryId = round.OrderHistoryId
                    }, ct);
                }

                await _uow.SaveChangesAsync(ct);
                await _uow.CommitTransactionAsync(ct);
            }
            catch
            {
                await _uow.RollbackTransactionAsync(ct);
                throw;
            }
            finally
            {
                locker.Release();
            }
        }
    }
}
