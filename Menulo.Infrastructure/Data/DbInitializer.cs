using Menulo.Infrastructure.Identity;
using Menulo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Menulo.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();

                CreateRolesAndAdminAsync(scope.ServiceProvider).Wait(); // Gọi async method và đợi hoàn thành
            }
        }

        //Tạo mặc định cho role và tài khoản superadmin
        private static async Task CreateRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Tạo role SuperAdmin nếu chưa tồn tại
            var roleCheckSuperAdmin = await roleManager.RoleExistsAsync("superadmin");
            if (!roleCheckSuperAdmin)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("superadmin"));
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("Role 'superadmin' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Error creating 'superadmin' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            // 2. Tạo role Admin nếu chưa tồn tại
            var roleCheckAdmin = await roleManager.RoleExistsAsync("admin");
            if (!roleCheckAdmin)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole("admin"));
                if (roleResult.Succeeded)
                {
                    Console.WriteLine("Role 'admin' created successfully.");
                }
                else
                {
                    Console.WriteLine($"Error creating 'admin' role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }

            // 3. Tạo tài khoản SuperAdmin mặc định nếu chưa tồn tại
            var superAdminUser = await userManager.FindByNameAsync("superadmin"); // Tìm theo Username
            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "superadmin",
                    NormalizedUserName = "SUPERADMIN"
                };
                var createSuperAdmin = await userManager.CreateAsync(superAdminUser, "Admin123@"); // Đặt mật khẩu mặc định

                if (createSuperAdmin.Succeeded)
                {
                    Console.WriteLine("Default 'superadmin' user created successfully.");

                    // Gán role 'superadmin' cho user này
                    var addToRoleResult = await userManager.AddToRoleAsync(superAdminUser, "superadmin");
                    if (addToRoleResult.Succeeded)
                    {
                        Console.WriteLine("Default 'superadmin' user added to 'superadmin' role.");
                    }
                    else
                    {
                        Console.WriteLine($"Error adding default 'superadmin' user to role: {string.Join(", ", addToRoleResult.Errors.Select(e => e.Description))}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error creating default 'superadmin' user: {string.Join(", ", createSuperAdmin.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
