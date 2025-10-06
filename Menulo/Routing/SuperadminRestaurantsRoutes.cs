using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Menulo.Routing
{
    public sealed class SuperadminRestaurantsRoutes : IConfigureOptions<RazorPagesOptions>
    {
        public void Configure(RazorPagesOptions options)
        {
            options.Conventions.AddPageRoute("/Superadmin/Restaurants/Index", "sa/ds-nha-hang");
            options.Conventions.AddPageRoute("/Superadmin/Restaurants/Create", "sa/ds-nha-hang/tao-moi");
            options.Conventions.AddPageRoute("/Superadmin/Restaurants/Edit", "sa/ds-nha-hang/{id:int}/chinh-sua");
        }
    }
}
