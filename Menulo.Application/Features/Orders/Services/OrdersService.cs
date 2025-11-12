using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Orders.Dtos;
using Menulo.Application.Features.Orders.Interfaces;

namespace Menulo.Application.Features.Orders.Services
{
    public sealed class OrdersService : IOrdersService
    {
        private readonly IUnitOfWork _uow;

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
    }
}
