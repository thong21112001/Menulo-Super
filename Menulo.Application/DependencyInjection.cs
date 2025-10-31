using Menulo.Application.Common.Mappings;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Application.Features.Categories.Services;
using Menulo.Application.Features.MenuItems.Interfaces;
using Menulo.Application.Features.MenuItems.Services;
using Menulo.Application.Features.ResTables.Interfaces;
using Menulo.Application.Features.ResTables.Services;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Application.Features.Restaurants.Services;
using Menulo.Application.Features.Sales.Interfaces;
using Menulo.Application.Features.Sales.Services;
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
            services.AddAutoMapper(typeof(ResTableProfile).Assembly);
            services.AddAutoMapper(typeof(SaleProfile).Assembly);
            services.AddAutoMapper(typeof(MenuItemsProfile).Assembly);

            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IRestaurantService, RestaurantService>();
            services.AddScoped<IResTablesService, ResTablesService>();
            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<IMenuItems, MenuItemsService>();

            return services;
        }
    }
}
