using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Domain.Entities;

namespace Menulo.Application.Features.Restaurants.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRepository<Restaurant> _repo;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;


        public RestaurantService(IUnitOfWork uow,
            IMapper mapper, ICurrentUser currentUser)
        {
            _uow = uow;
            _repo = _uow.Repository<Restaurant>();
            _mapper = mapper;
            _currentUser = currentUser;
        }


        public Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<RestaurantDto> UpdateAsync(UpdateRestaurantDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int restaurantId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<RestaurantDto?> GetByIdAsync(int restaurantId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Restaurant> GetQueryableRestaurantsForCurrentUser()
        {
            throw new NotImplementedException();
        }
    }
}
