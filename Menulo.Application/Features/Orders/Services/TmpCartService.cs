using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Orders.Dtos;
using Menulo.Application.Features.Orders.Interfaces;

namespace Menulo.Application.Features.Orders.Services
{
    public sealed class TmpCartService : ITmpCartService
    {
        private readonly IUnitOfWork _uow;

        public TmpCartService(IUnitOfWork uow)
        {
            _uow = uow;
        }



        public Task AddAsync(int restaurantId, int tableId, int itemId, int quantity, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<OrderItemLiteDto>> GetAsync(int restaurantId, int tableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllAsync(int restaurantId, int tableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(int itemsTmpId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
