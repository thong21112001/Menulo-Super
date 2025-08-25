namespace Menulo.Configuration
{
    public static class AuthenticationConfiguration
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services)
        {
            //ConfigureApplicationCookie: dùng để làm việc với asp.net core identity
            //AddAuthentication().AddCookie(): dùng để tự cấu hình cookie không liên quan đến identity
            services.ConfigureApplicationCookie(options =>
            {
                // đường dẫn trang login trong scaffold Identity
                options.LoginPath = "/Identity/Account/Login";
                // (tuỳ chọn) trang AccessDenied
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                // Thêm cấu hình thời gian ở đây:
                options.ExpireTimeSpan = TimeSpan.FromHours(8); // đặt 8 tiếng cho chuẩn với người dùng
                options.SlidingExpiration = true; // Gia hạn khi người dùng hoạt động
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
