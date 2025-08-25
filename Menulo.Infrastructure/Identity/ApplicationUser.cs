using Microsoft.AspNetCore.Identity;

namespace Menulo.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<string>
    {
        public string FullName { get; set; } = string.Empty;

        public int? RestaurantId { get; set; }
    }
}
