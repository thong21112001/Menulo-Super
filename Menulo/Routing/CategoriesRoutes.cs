using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Menulo.Routing
{
    public sealed class CategoriesRoutes : IConfigureOptions<RazorPagesOptions>
    {
        public void Configure(RazorPagesOptions options)
        {
            options.Conventions.AddPageRoute("/Categories/Index", "ds-danh-muc");
            options.Conventions.AddPageRoute("/Categories/Create", "ds-danh-muc/tao-moi");
            options.Conventions.AddPageRoute("/Categories/Edit", "ds-danh-muc/{id:int}/chinh-sua");
        }
    }
}
