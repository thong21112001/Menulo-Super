using Menulo.Application.Common.Interfaces;
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
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public Category Category { get; set; } = default!;

        public string? RestaurantName { get; set; }


        public CreateModel(IUnitOfWork uow, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _userManager = userManager;
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

            if (!string.IsNullOrEmpty(Category.CategoryName))
            {
                Category.CategoryName = CleanItemName(Category.CategoryName);
            }

            if (!ModelState.IsValid)
            {
                var loadDataResult = await AuthorizeAndLoadRestaurantData();
                if (loadDataResult != null)
                {
                    return loadDataResult;
                }

                var errorMessages = ModelState.Values
                              .SelectMany(modelStateEntry => modelStateEntry.Errors)
                              .Select(error => error.ErrorMessage)
                              .ToList();

                TempData.SetError("Lỗi: " + string.Join(" | ", errorMessages));
                return Page();
            }

            await _uow.Repository<Category>().AddAsync(Category);
            await _uow.SaveChangesAsync();

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
                    Category.RestaurantId = user.RestaurantId.Value;
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
