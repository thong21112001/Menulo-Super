using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Menulo.Routing
{
    public sealed class RoleSaleRoutes : IConfigureOptions<RazorPagesOptions>
    {
        public void Configure(RazorPagesOptions options)
        {
            options.Conventions.AddPageRoute("/RoleSale/CreateRestaurant/Index", "sale/ds-nha-hang");
            options.Conventions.AddPageRoute("/RoleSale/CreateRestaurant/Create", "sale/ds-nha-hang/tao-moi");
        }
    }
}
