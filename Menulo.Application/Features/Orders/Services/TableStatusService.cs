using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Orders.Dtos;
using Menulo.Application.Features.Orders.Interfaces;

namespace Menulo.Application.Features.Orders.Services
{
    public sealed class TableStatusService : ITableStatusService
    {
        private readonly IUnitOfWork _uow;

        public TableStatusService(IUnitOfWork uow)
        {
            _uow = uow;
        }



        public Task<IReadOnlyList<TableLiveStatusDto>> GetLiveStatusAsync(int restaurantId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
