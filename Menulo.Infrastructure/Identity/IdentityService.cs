using Menulo.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public IdentityService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResultDto> CreateUserAsync(
            string userName,
            string password,
            string email,
            string fullName,
            string phoneNumber,
            string role)
        {
            // Kiểm tra xem role có tồn tại không, nếu không thì tạo
            if (await _roleManager.FindByNameAsync(role) == null)
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = userName,
                Email = email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                RestaurantId = null // Tài khoản Sale không gắn với nhà hàng cụ thể
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return IdentityResultDto.Failure(result.Errors.Select(e => e.Description));
            }

            // Gán role cho user
            await _userManager.AddToRoleAsync(user, role);

            return IdentityResultDto.Success(user.Id);
        }

        public async Task<bool> IsUsernameTakenAsync(string username, CancellationToken ct = default)
        {
            return await _userManager.FindByNameAsync(username) != null;
        }

        public async Task<bool> IsEmailTakenAsync(string email, CancellationToken ct = default)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<bool> IsPhoneTakenAsync(string phoneNumber, CancellationToken ct = default)
        {
            // Kiểm tra xem có bất kỳ user nào trong DB có SĐT này không
            return await _userManager.Users
                .AnyAsync(u => u.PhoneNumber == phoneNumber, ct);
        }
    }
}
