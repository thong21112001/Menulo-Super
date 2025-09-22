using System.Security.Claims;

namespace Menulo.Application.Common.Interfaces
{
    public interface ICurrentUser
    {
        ClaimsPrincipal? User { get; }
        int? RestaurantId { get; }
        bool IsSuperAdmin { get; }
    }
}
