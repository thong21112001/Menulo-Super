using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Categories.Dtos;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Application.Features.ResTables.Interfaces;
using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Domain.Entities;
using Menulo.Extensions;
using Menulo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Menulo.Pages.RestaurantTables
{
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IResTablesService _svc;
        private readonly IUnitOfWork _uow;

        [BindProperty]
        public ResTableRequest.Update Input { get; set; } = new();

        public string? RestaurantName { get; set; }



        public EditModel(UserManager<ApplicationUser> userManager, IResTablesService svc, IUnitOfWork uow)
        {
            _userManager = userManager;
            _svc = svc;
            _uow = uow;
        }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var authorizationResult = await AuthorizeAndLoadRestaurantData();

            if (authorizationResult != null)
            {
                return authorizationResult;
            }

            var resDb = await _svc.GetByIdAsync(id.Value);

            if (resDb == null)
                return NotFound();

            Input = new ResTableRequest.Update
            {
                TableId = resDb.TableId,
                RestaurantId = resDb.RestaurantId,
                TableCode = resDb.TableCode,
                Description = resDb.Description,
            };

            return Page();
        }


        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            var authorizationResult = await AuthorizeAndSetRestaurantId();

            if (authorizationResult != null)
            {
                return authorizationResult;
            }

            if (!string.IsNullOrWhiteSpace(Input.Description))
                Input.Description = CleanItemName(Input.Description);

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

            // Chuẩn bị DTO với các trường text
            var dto = new UpdateResTableDto(
                TableId: Input.TableId,
                RestaurantId: Input.RestaurantId,
                TableCode: Input.TableCode ?? string.Empty,
                Description: Input.Description
            );

            try
            {
                await _svc.UpdateAsync(dto, ct);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Không tìm thấy bàn để cập nhật.");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi (ex)
                await AuthorizeAndLoadRestaurantData(); // Tải lại dữ liệu cho form
                TempData.SetError("Đã xảy ra lỗi khi cập nhật: " + ex.Message);
                return Page();
            }

            TempData.SetSuccess("Cập nhật thông tin bàn thành công!");
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

            return System.Text.RegularExpressions.Regex.Replace(itemName.Trim(), @"\s+", " ");
        }
        #endregion
    }
}
