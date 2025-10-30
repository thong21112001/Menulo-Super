using Menulo.Application.Common.Interfaces;
using Menulo.Application.Common.Options;
using Menulo.Infrastructure.External.Storage;
using Menulo.Infrastructure.Identity;
using Menulo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Menulo.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));


            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Cấu hình Password
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                // Cấu hình Lockout (vô hiệu hóa)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.Zero;
                options.Lockout.MaxFailedAccessAttempts = 999;
                options.Lockout.AllowedForNewUsers = false;
                // Cấu hình User (bỏ qua yêu cầu email)
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                // Cấu hình SignIn (vô hiệu hóa yêu cầu xác nhận email/số điện thoại)
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // Đăng ký Unit of Work, sẽ quản lý các repository
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IIdentityService, IdentityService>();

            //services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Bind options từ section "GoogleOAuth"
            services.AddOptions<GoogleOAuthOptions>()
                    .Bind(config.GetSection("GoogleOAuth"))
                    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId)
                                && !string.IsNullOrWhiteSpace(o.ClientSecret)
                                && !string.IsNullOrWhiteSpace(o.RedirectUri)
                                && !string.IsNullOrWhiteSpace(o.ApplicationName),
                              "GoogleOAuth options are missing");

            services.AddSingleton<ITokenStore, FileTokenStore>();

            services.AddSingleton<IImageStorageService, GoogleDriveOAuthImageStorageService>();

            return services;
        }
    }
}
