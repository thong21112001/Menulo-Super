using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Features.Restaurants.Interfaces
{
    public interface IRestaurantService
    {
        Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto, CancellationToken ct = default);
        Task<RestaurantDto> UpdateAsync(UpdateRestaurantDto dto, CancellationToken ct = default);
        Task DeleteAsync(int restaurantId, CancellationToken ct = default);
        Task<RestaurantDto?> GetByIdAsync(int restaurantId, CancellationToken ct = default);
        IQueryable<Restaurant> GetQueryableRestaurantsForCurrentUser();
    }
}
