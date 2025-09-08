using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Infrastructure.Identity;
using Menulo.WebAPI.Api.Contracts.Categories;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Menulo.WebAPI.Api.Contracts.DataTables.DataTablesModels;

namespace Menulo.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigin")]
    public sealed class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly UserManager<ApplicationUser> _userManager; // nếu bạn dùng Identity
        public CategoriesController(ICategoryService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        // DataTables endpoint
        [HttpPost("datatable")]
        public async Task<ActionResult<DataTablesResponse<CategoryResponse>>> GetDataTable([FromBody] DataTablesRequest req, CancellationToken ct)
        {
            // Quyền (superadmin được thấy tất cả)
            var user = await _userManager.GetUserAsync(User);
            var isSuperAdmin = user != null && await _userManager.IsInRoleAsync(user, "superadmin");
            int? restaurantId = isSuperAdmin ? null : user?.RestaurantId;

            int pageSize = req.length <= 0 ? 10 : req.length;
            int page = (req.start / pageSize) + 1;

            string? orderBy = null; bool orderDesc = false;
            if (req.order.Count > 0)
            {
                var ord = req.order[0];
                var col = req.columns[ord.column];
                orderBy = string.IsNullOrWhiteSpace(col.name) ? col.data : col.name;
                orderDesc = string.Equals(ord.dir, "desc", StringComparison.OrdinalIgnoreCase);
            }

            var (items, total) = await _service.GetPagedAsync(
                page, pageSize, req.search?.value, orderBy, orderDesc, isSuperAdmin, restaurantId, ct);

            var data = items.Select(x => new CategoryResponse(x.CategoryId, x.CategoryName, x.RestaurantId, x.RestaurantName));

            return Ok(new DataTablesResponse<CategoryResponse>
            {
                draw = req.draw,
                recordsTotal = total,
                recordsFiltered = total,
                data = data
            });
        }

        // REST CRUD cơ bản (cho Flutter / Web)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryResponse>> Get(int id, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(id, ct);
            return dto is null
                ? NotFound()
                : new CategoryResponse(dto.CategoryId, dto.CategoryName, dto.RestaurantId, dto.RestaurantName);
        }

        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> Create([FromBody] CreateCategoryRequest req, CancellationToken ct)
        {
            var dto = await _service.CreateAsync(new(req.CategoryName, req.RestaurantId), ct);
            return CreatedAtAction(nameof(Get), new { id = dto.CategoryId },
                new CategoryResponse(dto.CategoryId, dto.CategoryName, dto.RestaurantId, dto.RestaurantName));
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CategoryResponse>> Update(int id, [FromBody] UpdateCategoryRequest req, CancellationToken ct)
        {
            if (id != req.CategoryId) return BadRequest("Id mismatch");
            var dto = await _service.UpdateAsync(new(req.CategoryId, req.CategoryName, req.RestaurantId), ct);
            return new CategoryResponse(dto.CategoryId, dto.CategoryName, dto.RestaurantId, dto.RestaurantName);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}
