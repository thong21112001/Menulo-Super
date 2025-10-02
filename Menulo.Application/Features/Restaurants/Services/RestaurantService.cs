using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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


        public async Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto, CancellationToken ct = default)
        {
            var entity = _mapper.Map<Restaurant>(dto);
            await _repo.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);
            return _mapper.Map<RestaurantDto>(entity);
        }

        public Task<RestaurantDto> UpdateAsync(UpdateRestaurantDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int restaurantId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<RestaurantDetailsDto?> GetByIdAsync(int restaurantId, CancellationToken ct = default)
        {
            // Bước 1: Lấy toàn bộ entity 'Restaurant' từ database về.
            // Lúc này, LogoImage vẫn là một mảng byte (byte[]).
            var entity = await _repo.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RestaurantId == restaurantId, ct);

            // Nếu không tìm thấy entity thì trả về null luôn.
            if (entity is null)
            {
                return null;
            }

            // Bước 2: Dùng _mapper.Map() để thực hiện việc mapping trong bộ nhớ.
            // Ở bước này, AutoMapper sẽ chạy code C# và thực hiện Convert.ToBase64String() một cách bình thường.
            var dto = _mapper.Map<RestaurantDetailsDto>(entity);

            return dto;
        }

        public IQueryable<Restaurant> GetQueryableRestaurantsForCurrentUser()
        {
            var query = _repo.GetQueryable();

            return query;
        }
    }
}
