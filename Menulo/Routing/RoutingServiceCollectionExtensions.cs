using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Menulo.Routing
{
    /// <summary>
    /// Extension để đăng ký tất cả UI routes:
    /// </summary>
    public static class RoutingServiceCollectionExtensions
    {
        public static IServiceCollection AddMenuloUiRoutes(this IServiceCollection services)
        {
            services.AddTransient<IConfigureOptions<RazorPagesOptions>, CategoriesRoutes>();
            services.AddTransient<IConfigureOptions<RazorPagesOptions>, SuperadminRestaurantsRoutes>();
            return services;
        }
    }
}
