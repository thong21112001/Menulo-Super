using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Categories.Dtos;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Domain.Entities;
using Menulo.Extensions;
using Menulo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICategoryService _svc;
        private readonly IUnitOfWork _uow;

        [BindProperty]
        public CreateCategoryRequest Input { get; set; } = new();

        public string? RestaurantName { get; set; }


        public CreateModel(IUnitOfWork uow, UserManager<ApplicationUser> userManager, ICategoryService svc)
        {
            _userManager = userManager;
            _svc = svc;
            _uow = uow;
        }



        public async Task<IActionResult> OnGetAsync()
        {
            var authorizationResult = await AuthorizeAndLoadRestaurantData();
            if (authorizationResult != null)
            {
                return authorizationResult;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var authorizationResult = await AuthorizeAndSetRestaurantId();
            if (authorizationResult != null)
            {
                return authorizationResult;
            }

            if (!string.IsNullOrWhiteSpace(Input.CategoryName))
                Input.CategoryName = CleanItemName(Input.CategoryName);

            if (!ModelState.IsValid)
            {
                await AuthorizeAndLoadRestaurantData();

                var errorMessages = ModelState.Values
                              .SelectMany(modelStateEntry => modelStateEntry.Errors)
                              .Select(error => error.ErrorMessage)
                              .ToList();

                TempData.SetError("Lỗi: " + string.Join(" | ", errorMessages));
                return Page();
            }

            // Map Request -> DTO (đúng type service cần)
            var dto = new CreateCategoryDto(
                CategoryName: Input.CategoryName,
                RestaurantId: Input.RestaurantId
            );

            await _svc.CreateAsync(dto);
            TempData.SetSuccess("Tạo mới danh mục thành công!");
            return RedirectToPage("./Index");
        }

        #region Viết phương thức xử lý
        private async Task<IActionResult?> AuthorizeAndLoadRestaurantData()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var isSuperAdmin = await _userManager.IsInRoleAsync(user, "superadmin");

            if (isSuperAdmin)
            {
                ViewData["RestaurantId"] = new SelectList(
                   await _uow.Repository<Restaurant>().GetAllAsync(), "RestaurantId", "Name");
            }
            else if (user.RestaurantId.HasValue)
            {
                var rest = await _uow.Repository<Restaurant>().GetQueryable()
                                    .Where(r => r.RestaurantId == user.RestaurantId.Value)
                                    .FirstOrDefaultAsync();

                if (rest == null)
                {
                    return Forbid();
                }
                RestaurantName = rest.Name;
            }
            else
            {
                return Forbid();
            }

            return null;
        }

        private async Task<IActionResult?> AuthorizeAndSetRestaurantId()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Forbid();
            }

            var isSuperAdmin = await _userManager.IsInRoleAsync(user, "superadmin");

            if (!isSuperAdmin)
            {
                // Ép gán đúng để tránh client spoofing
                if (user.RestaurantId.HasValue)
                    Input.RestaurantId = user.RestaurantId.Value;
                else
                    return Forbid();
            }
            return null;
        }

        private string CleanItemName(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return string.Empty;

            // Loại bỏ khoảng trống đầu/cuối và thay thế nhiều khoảng trống liên tiếp bằng 1 khoảng trống
            return System.Text.RegularExpressions.Regex.Replace(itemName.Trim(), @"\s+", " ");
        }
        #endregion
    }
}
