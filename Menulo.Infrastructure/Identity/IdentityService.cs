using AutoMapper;
using AutoMapper.QueryableExtensions;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Sales.Dtos;
using Menulo.Domain.Entities;
using Menulo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Menulo.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        //private readonly IMapper _mapper;
        private readonly AppDbContext _context; // Giữ lại cho các truy vấn Identity phức tạp
        private readonly IUnitOfWork _uow;


        public IdentityService(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager, 
            AppDbContext context, IUnitOfWork uow)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _uow = uow;
        }

        #region Phần này dành cho superadmin tạo Sale
        // Tạo user với role tương ứng (sale)
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

        public IQueryable<SaleRowDto> GetUsersAsQueryable(string roleName)
        {
            var role = _roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                return Enumerable.Empty<SaleRowDto>().AsQueryable();
            }

            // Sử dụng DbContext để join với _context.UserRoles.
            var query = from user in _userManager.Users
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        where userRole.RoleId == role.Id
                        join restaurant in _context.Restaurants
                            on user.Id equals restaurant.CreatedBySaleId // Dựa trên FK
                            into userRestaurants
                        select new SaleRowDto
                        {
                            UserId = user.Id,
                            FullName = user.FullName,
                            Username = user.UserName ?? "-",
                            Email = user.Email ?? "-",
                            PhoneNumber = user.PhoneNumber ?? "-",
                            // Đếm số lượng nhà hàng
                            // userRestaurants là một IGrouping<...>
                            RestaurantCount = userRestaurants.Count()
                        };

            return query;
        }

        public async Task<SaleDto?> GetUserByIdAsync(string userId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new SaleDto(
                user.Id,
                user.FullName,
                user.UserName ?? "Lỗi lấy tên tài khoản",
                user.Email ?? "Lỗi lấy email",
                user.PhoneNumber ?? "Lỗi lấy số điện thoại"
            );
        }

        public async Task<IdentityResultDto> DeleteUserAsync(string userId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResultDto.Failure(new[] { "Không tìm thấy tài khoản." });
            }

            var restaurantRepo = _uow.Repository<Restaurant>();
            var hasRelatedRestaurants = await restaurantRepo.GetQueryable()
                .AnyAsync(r => r.CreatedBySaleId == userId, ct);

            if (hasRelatedRestaurants)
            {
                return IdentityResultDto.Failure(new[] { "Không thể xóa tài khoản Sale này vì đã phát sinh dữ liệu." });
            }

            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded
                ? IdentityResultDto.Success(userId)
                : IdentityResultDto.Failure(result.Errors.Select(e => e.Description));
        }
        #endregion



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
