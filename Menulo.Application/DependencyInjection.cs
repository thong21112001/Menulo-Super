using Menulo.Application.Common.Mappings;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Application.Features.Categories.Services;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Application.Features.Restaurants.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Menulo.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Quét toàn bộ assembly Application để nạp các Profile
            services.AddAutoMapper(typeof(CategoryProfile).Assembly);
            services.AddAutoMapper(typeof(RestaurantProfile).Assembly);

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IRestaurantService, RestaurantService>();

            return services;
        }
    }
}
