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
            services.AddTransient<IConfigureOptions<RazorPagesOptions>, ResTableRoutes>();
            services.AddTransient<IConfigureOptions<RazorPagesOptions>, SuperadminSalesRoutes>();
            services.AddTransient<IConfigureOptions<RazorPagesOptions>, RoleSaleRoutes>();
            services.AddTransient<IConfigureOptions<RazorPagesOptions>, MenuItemsRoutes>();
            services.AddTransient<IConfigureOptions<RazorPagesOptions>, MenuRestaurantRoutes>();
            return services;
        }
    }
}
