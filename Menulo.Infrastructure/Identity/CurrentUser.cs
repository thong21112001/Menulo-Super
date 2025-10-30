using Menulo.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Menulo.Infrastructure.Identity
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUser(IHttpContextAccessor http) => _http = http;

        public ClaimsPrincipal? User => _http.HttpContext?.User;

        public int? RestaurantId
            => int.TryParse(User?.FindFirst("RestaurantId")?.Value, out var id) ? id : (int?)null;

        public bool IsSuperAdmin => User?.IsInRole("superadmin") == true;

        /// <summary>
        /// Lấy ID (dạng string GUID) của ApplicationUser đang đăng nhập.
        /// Trả về null nếu người dùng chưa đăng nhập.
        /// </summary>
        public string? UserId
            => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
