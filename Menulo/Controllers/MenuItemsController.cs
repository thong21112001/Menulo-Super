using AutoMapper;
using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.MenuItems.Dtos;
using Menulo.Application.Features.MenuItems.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static Menulo.Application.Common.Contracts.DataTables.DataTablesModels;

namespace Menulo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "admin, superadmin")]
    public sealed class MenuItemsController : DataTablesControllerBase
    {
        private readonly IMenuItemsService _service;
        private readonly ICurrentUser _currentUser;

        public MenuItemsController(
            IMenuItemsService service,
            IMapper mapper,
            ICurrentUser currentUser) : base(mapper)
        {
            _service = service;
            _currentUser = currentUser;
        }

        // API 1: Datatables server-side processing
        [HttpPost("datatable")]
        [ValidateAntiForgeryToken]
        public IActionResult GetDataTable([FromBody] DataTablesRequest request)
        {
            // Lấy IQueryable từ service
            IQueryable<MenuItemRowDto> source = _service.GetQueryableMenuItemsForCurrentUser();

            // Xây dựng biểu thức tìm kiếm
            Expression<Func<MenuItemRowDto, bool>>? searchPredicate = null;
            var searchValue = request.Search?.Value;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var kw = searchValue.Trim().ToLower();
                searchPredicate = m =>
                    m.ItemName.ToLower().Contains(kw) ||
                    m.CategoryName.ToLower().Contains(kw) ||
                    (m.Description != null && m.Description.ToLower().Contains(kw)) ||
                    (_currentUser.IsSuperAdmin && m.RestaurantName != null && m.RestaurantName.ToLower().Contains(kw));
            }

            return GetDataTableResult<MenuItemRowDto, MenuItemRowDto>(source, request, searchPredicate);
        }

        // API 2: Toggle Availability (Chuyển đổi trạng thái món)
        [HttpPatch("{id:int}/toggle-availability")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id, CancellationToken ct)
        {
            try
            {
                var newStatus = await _service.ToggleMenuItemAvailabilityAsync(id, ct);

                return Ok(new
                {
                    success = true,
                    message = "Đã cập nhật trạng thái thành công.",
                    isAvailable = newStatus
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, message = "Không tìm thấy món ăn." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
