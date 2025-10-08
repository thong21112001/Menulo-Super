using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Domain.Entities;

namespace Menulo.Application.Features.Restaurants.Interfaces
{
    public interface IRestaurantService
    {
        Task<RestaurantDto> CreateAsync(CreateRestaurantDto dto, CancellationToken ct = default);
        Task<RestaurantDto> UpdateAsync(UpdateRestaurantDto dto, CancellationToken ct = default);
        Task DeleteAsync(int restaurantId, CancellationToken ct = default);
        Task<RestaurantDetailsDto?> GetByIdAsync(int restaurantId, CancellationToken ct = default);
        IQueryable<Restaurant> GetQueryableRestaurantsForCurrentUser();

        Task<RestaurantDto> CreateWithLogoAsync(
            string name, string? address, string? phone,
            Stream logoStream, string logoFileName, string contentType,
            CancellationToken ct = default);

        Task<RestaurantDto> UpdateWithOptionalLogoAsync(UpdateRestaurantDto dto,
            Stream? newLogoStream, string? newLogoFileName, string? contentType,
            CancellationToken ct = default);
    }
}
