using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Menulo.Routing
{
    public sealed class MenuItemsRoutes : IConfigureOptions<RazorPagesOptions>
    {
        public void Configure(RazorPagesOptions options)
        {
            options.Conventions.AddPageRoute("/MenuItems/Index", "ds-mon-an");
            options.Conventions.AddPageRoute("/MenuItems/Create", "ds-mon-an/tao-moi");
            options.Conventions.AddPageRoute("/MenuItems/Edit", "ds-mon-an/{id:int}/chinh-sua");
        }
    }
}
