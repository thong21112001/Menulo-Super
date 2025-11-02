using AutoMapper;
using Menulo.Application.Features.Categories.Dtos;
using Menulo.Application.Features.Categories.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static Menulo.Application.Common.Contracts.DataTables.DataTablesModels;

namespace Menulo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class CategoriesController : DataTablesControllerBase
    {
        private readonly ICategoryService _service;

        public CategoriesController(ICategoryService service, IMapper mapper)
        : base(mapper)
        {
            _service = service;
        }


        // API 1: Datatables server-side processing
        [HttpPost("datatable")]
        [ValidateAntiForgeryToken]
        public IActionResult GetDataTable([FromBody] DataTablesRequest request, CancellationToken ct)
        {
            // Lấy IQueryable từ service
            IQueryable<Category> source = _service.GetQueryableCategoriesForCurrentUser();

            // Xây dựng biểu thức tìm kiếm
            Expression<Func<Category, bool>>? searchPredicate = null;
            var searchValue = request.Search?.Value;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var kw = searchValue.Trim().ToLower();
                // Lưu ý: nếu non-superadmin thì cột Restaurant không hiển thị, nhưng search vẫn OK
                searchPredicate = c =>
                    c.CategoryName.ToLower().Contains(kw) ||
                    (c.Restaurant != null && c.Restaurant.Name.ToLower().Contains(kw));
            }

            // Gọi phương thức của base để xử lý
            // Base sẽ map sang CategoryResponse.
            // Mapping sẽ ẩn RestaurantName với non-superadmin,
            // hoặc có thể để Service trả DTO đã ẩn sẵn.
            return GetDataTableResult<Category, CategoryResponse>(source, request, searchPredicate);
        }

        // API 2: Lấy dữ liệu để hiển thị cho view chi tiết và xóa
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryResponse>> Get(int id, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(id, ct);
            return dto is null
                ? NotFound()
                : Ok(new CategoryResponse(dto.CategoryId, dto.CategoryName, dto.RestaurantId, dto.Priority, dto.RestaurantName));
        }

        // API 3: Xóa dữ liệu
        [HttpDelete("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }

        // API 4: Lấy danh mục dựa trên id nhà hàng (dành cho Superadmin)
        [HttpGet("for-restaurant/{restaurantId:int}")]
        [ProducesResponseType(typeof(IEnumerable<object>), 200)]
        public async Task<IActionResult> GetCategoriesForRestaurant(int restaurantId, CancellationToken ct)
        {
            // Lấy IQueryable đã được lọc (bảo mật)
            var query = _service.GetQueryableCategoriesForCurrentUser();

            // Lọc thêm theo restaurantId mà Superadmin đã chọn
            // (Nếu admin gọi, nó sẽ lọc 2 lần, vẫn an toàn)
            var categories = await query
                .Where(c => c.RestaurantId == restaurantId)
                .Select(c => new { c.CategoryId, c.CategoryName, c.Priority }) // Trả về DTO ẩn danh
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.CategoryName)
                .ToListAsync(ct);

            return Ok(categories);
        }
    }
}
