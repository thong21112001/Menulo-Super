using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace Menulo.Configuration
{
    public static class AuthenticationConfiguration
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            //ConfigureApplicationCookie: dùng để làm việc với asp.net core identity
            //AddAuthentication().AddCookie(): dùng để tự cấu hình cookie không liên quan đến identity
            services.Configure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });


            // Sử dụng cache + Session
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
                options.Cookie.HttpOnly = true; // Cookie chỉ có thể truy cập qua HTTP
                options.Cookie.IsEssential = true; // Ngăn chặn JavaScript phía client truy cập vào cookie của session
            });

            return services;
        }
    }
}
