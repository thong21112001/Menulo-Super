using Menulo.Application.Common.Mappings;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Application.Features.Categories.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Menulo.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Quét toàn bộ assembly Application để nạp các Profile (trong đó có CategoryProfile)
            services.AddAutoMapper(typeof(CategoryProfile).Assembly);

            services.AddScoped<ICategoryService, CategoryService>();

            return services;
        }
    }
}
