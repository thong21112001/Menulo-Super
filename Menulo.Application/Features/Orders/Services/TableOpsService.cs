using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Orders.Dtos;
using Menulo.Application.Features.Orders.Interfaces;

namespace Menulo.Application.Features.Orders.Services
{
    public sealed class TableOpsService : ITableOpsService
    {
        private readonly IUnitOfWork _uow;

        public TableOpsService(IUnitOfWork uow)
        {
            _uow = uow;
        }



        public Task<IEnumerable<TableMiniDto>> GetActiveAsync(int restaurantId, int currentTableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TableMiniDto>> GetAvailableAsync(int restaurantId, int currentTableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task MergeAsync(int fromTableId, int toTableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task TransferAsync(int orderId, int toTableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
