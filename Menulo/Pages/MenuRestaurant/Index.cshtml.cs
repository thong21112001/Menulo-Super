using Menulo.Application.Features.MenuItems.Dtos;
using Menulo.Application.Features.MenuItems.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Menulo.Pages.MenuRestaurant
{
    public class IndexModel : PageModel
    {
        private readonly IMenuItemsService _menuItemService;

        public IndexModel(IMenuItemsService menuItemService)
        {
            _menuItemService = menuItemService;
        }

        // Dữ liệu trả về là DTO đã được nhóm sẵn
        public List<MenuCategoryGroupDto> MenuGroups { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            try
            {
                MenuGroups = await _menuItemService.GetMenuForCurrentUserAsync(ct);
                return Page();
            }
            catch (UnauthorizedAccessException)
            {
                return Challenge(); // Nếu user chưa được gán nhà hàng
            }
        }
    }
}
