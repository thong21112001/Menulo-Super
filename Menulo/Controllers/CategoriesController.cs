using AutoMapper;
using Menulo.Application.Features.Categories.Dtos;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Domain.Entities;
using Menulo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static Menulo.Application.Common.Contracts.DataTables.DataTablesModels;

namespace Menulo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CategoriesController : DataTablesControllerBase
    {
        private readonly ICategoryService _service;
        private readonly UserManager<ApplicationUser> _userManager;


        public CategoriesController(ICategoryService service, UserManager<ApplicationUser> userManager, IMapper mapper)
        : base(mapper)
        {
            _service = service;
            _userManager = userManager;
        }



        [HttpPost("datatable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetDataTable([FromForm] DataTablesRequest request, CancellationToken ct)
        {
            var user = await _userManager.GetUserAsync(User);
            var isSuperAdmin = user != null && await _userManager.IsInRoleAsync(user, "superadmin");
            int? restaurantId = isSuperAdmin ? null : user?.RestaurantId;

            // Lấy IQueryable từ service
            IQueryable<Category> source = _service.GetQueryableCategories(isSuperAdmin, restaurantId);

            // Xây dựng biểu thức tìm kiếm
            Expression<Func<Category, bool>>? searchPredicate = null;
            var searchValue = request.Search?.Value;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var kw = searchValue.Trim().ToLower();
                searchPredicate = c => c.CategoryName.ToLower().Contains(kw)
                                   || (c.Restaurant != null && c.Restaurant.Name.ToLower().Contains(kw));
            }

            // Gọi phương thức của base để xử lý
            return GetDataTableResult<Category, CategoryResponse>(source, request, searchPredicate);
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
