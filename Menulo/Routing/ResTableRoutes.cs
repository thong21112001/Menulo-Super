using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Menulo.Routing
{
    public sealed class ResTableRoutes : IConfigureOptions<RazorPagesOptions>
    {
        public void Configure(RazorPagesOptions options)
        {
            options.Conventions.AddPageRoute("/RestaurantTables/Index", "ds-ban");
            options.Conventions.AddPageRoute("/RestaurantTables/Create", "ds-ban/tao-moi");
            options.Conventions.AddPageRoute("/RestaurantTables/Edit", "ds-ban/{id:int}/chinh-sua");
            options.Conventions.AddPageRoute("/TableOrders/Index", "theo-doi-ban");
        }
    }
}
