using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Menulo.Routing
{
    public sealed class SuperadminSalesRoutes : IConfigureOptions<RazorPagesOptions>
    {
        public void Configure(RazorPagesOptions options)
        {
            options.Conventions.AddPageRoute("/Superadmin/Sales/Index", "sa/ds-sale");
            options.Conventions.AddPageRoute("/Superadmin/Sales/Create", "sa/ds-sale/tao-moi");
            options.Conventions.AddPageRoute("/Superadmin/Sales/Edit", "sa/ds-sale/{id:int}/chinh-sua");
        }
    }
}
