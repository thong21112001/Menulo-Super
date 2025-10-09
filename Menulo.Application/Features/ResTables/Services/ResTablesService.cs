using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Application.Features.ResTables.Interfaces;
using Menulo.Domain.Entities;

namespace Menulo.Application.Features.ResTables.Services
{
    public class ResTablesService : IResTablesService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<RestaurantTable> _repo;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;


        public ResTablesService(IUnitOfWork uow,
            IRepository<RestaurantTable> repo, IMapper mapper, ICurrentUser currentUser)
        {
            _uow = uow;
            _repo = repo;
            _mapper = mapper;
            _currentUser = currentUser;
        }


        public Task<ResTableDto> CreateAsync(CreateResTableDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ResTableDto> UpdateAsync(UpdateResTableDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int tableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<ResTableResponse?> GetByIdAsync(int tableId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IQueryable<RestaurantTable> GetQueryableRestaurantTableForCurrentUser()
        {
            throw new NotImplementedException();
        }
    }
}
