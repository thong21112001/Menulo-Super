using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Application.Features.MenuItems.Dtos;
using Menulo.Application.Features.MenuItems.Interfaces;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Menulo.Pages.MenuItems
{
    [Authorize(Roles = "admin, superadmin")]
    public class CreateModel : PageModel
    {
        private readonly IMenuItemsService _menuItemService;
        private readonly ICategoryService _categoryService;
        private readonly IRestaurantService _restaurantService;
        private readonly ICurrentUser _currentUser;

        public CreateModel(
            IMenuItemsService menuItemService,
            ICategoryService categoryService,
            ICurrentUser currentUser,
            IRestaurantService restaurantService)
        {
            _menuItemService = menuItemService;
            _categoryService = categoryService;
            _currentUser = currentUser;
            _restaurantService = restaurantService;
        }

        [BindProperty]
        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn một danh mục.")]
        public int CategoryId { get; set; }

        [BindProperty]
        public List<ItemInputModel> Items { get; set; } = new();

        public SelectList CategoryList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public SelectList RestaurantList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());
        public bool IsSuperAdmin { get; set; }


        public class ItemInputModel
        {
            [Required(ErrorMessage = "Tên món không được trống")]
            public string ItemName { get; set; } = string.Empty;

            public string? Price { get; set; }

            public string? Description { get; set; }

            public IFormFile? Image { get; set; }
        }



        public async Task<IActionResult> OnGetAsync()
        {
            IsSuperAdmin = _currentUser.IsSuperAdmin;

            await PopulateCategoryListAsync();

            // Khởi tạo 1 hàng trống
            Items.Add(new ItemInputModel());

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCategoryListAsync();
                return Page();
            }

            // Lấy RestaurantId của category được chọn một cách an toàn
            var category = await _categoryService.GetByIdAsync(CategoryId, ct);

            if (category == null)
            {
                // (Xử lý lỗi nếu user cố tình sửa CategoryId)
                ModelState.AddModelError(string.Empty, "Danh mục không hợp lệ.");
                await PopulateCategoryListAsync();
                return Page();
            }

            int successCount = 0;
            var errorMessages = new List<string>();

            // Lặp qua từng món ăn được gửi lên
            foreach (var item in Items)
            {
                if (string.IsNullOrWhiteSpace(item.ItemName)) continue; // Bỏ qua hàng trống

                try
                {
                    // 1. Chuẩn bị DTO "sạch"
                    var dto = new CreateMenuItemDto(
                        CategoryId,
                        category.RestaurantId,
                        item.ItemName.Trim(),
                        ParsePrice(item.Price!), // Helper parse giá
                        item.Description
                    );

                    // 2. Chuẩn bị Stream ảnh
                    Stream? imageStream = null;
                    string? imageFileName = null;
                    string? contentType = null;

                    if (item.Image != null && item.Image.Length > 0)
                    {
                        imageStream = item.Image.OpenReadStream();
                        imageFileName = item.Image.FileName;
                        contentType = item.Image.ContentType;
                    }

                    // 3. Gọi Service
                    await _menuItemService.CreateMenuItemAsync(
                        dto, imageStream, imageFileName, contentType, ct
                    );

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"Lỗi khi tạo món '{item.ItemName}': {ex.Message}");
                }
                finally
                {
                    // Đóng stream sau khi dùng
                    //await imageStream?.DisposeAsync();
                }
            }

            // 4. Thông báo kết quả
            if (successCount > 0)
                TempData.SetSuccess($"Đã tạo thành công {successCount} món ăn.");

            if (errorMessages.Any())
                TempData.SetError("Đã xảy ra lỗi: " + string.Join(" | ", errorMessages));

            return RedirectToPage("./Index"); // Chuyển về trang danh sách
        }



        #region Viết phương thức xử lý
        private async Task PopulateCategoryListAsync()
        {
            IsSuperAdmin = _currentUser.IsSuperAdmin;

            if (IsSuperAdmin)
            {
                var restaurants = await _restaurantService.GetQueryableRestaurantsForCurrentUser()
                    .AsNoTracking()
                    .Select(r => new { r.RestaurantId, r.Name })
                    .ToListAsync();

                RestaurantList = new SelectList(restaurants, "RestaurantId", "Name");

                // Để trống danh sách danh mục, chờ JS tải
                CategoryList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            else
            {
                var categoryQuery = _categoryService.GetQueryableCategoriesForCurrentUser();

                var categories = await categoryQuery
                    .Select(c => new { c.CategoryId, c.CategoryName })
                    .ToListAsync();

                CategoryList = new SelectList(categories, "CategoryId", "CategoryName");

                RestaurantList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private decimal? ParsePrice(string priceStr)
        {
            if (string.IsNullOrWhiteSpace(priceStr)) return null;

            if (decimal.TryParse(priceStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var price))
            {
                return price >= 0 ? price : null;
            }
            return null;
        }
        #endregion
    }
}
