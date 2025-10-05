using AutoMapper;
using Menulo.Application.Features.Restaurants.Dtos;
using Menulo.Application.Features.Restaurants.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static Menulo.Application.Common.Contracts.DataTables.DataTablesModels;

namespace Menulo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RestaurantsController : DataTablesControllerBase
    {
        private readonly IRestaurantService _service;

        public RestaurantsController(IMapper mapper, IRestaurantService service) : base(mapper)
        {
            _service = service;
        }


        // API 1: Datatables server-side processing
        [HttpPost("datatable")]
        [ValidateAntiForgeryToken]
        public IActionResult GetDataTable([FromBody] DataTablesRequest request, CancellationToken ct)
        {
            // Lấy IQueryable từ service
            IQueryable<Restaurant> source = _service.GetQueryableRestaurantsForCurrentUser();

            // Xây dựng biểu thức tìm kiếm
            Expression<Func<Restaurant, bool>>? searchPredicate = null;
            var searchValue = request.Search?.Value;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var kw = searchValue.Trim().ToLower();
                searchPredicate = c =>
                    c.Name.ToLower().Contains(kw) ||
                    (c.Phone != null && c.Phone.Contains(kw)) ||
                    (c.Address != null && c.Address.Contains(kw));
            }

            return GetDataTableResult<Restaurant, RestaurantRowDto>(source, request, searchPredicate);
        }

        // API 2: Lấy dữ liệu để hiển thị cho view chi tiết và xóa
        [HttpGet("{id:int}")]
        public async Task<ActionResult<RestaurantDetailsDto>> Get(int id, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(id, ct);

            return dto is null
                    ? NotFound()
                    : Ok(dto);
        }

        // API 3: Xóa dữ liệu
        [HttpDelete("{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            try
            {
                await _service.DeleteAsync(id, ct);
                return NoContent(); // 204
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                // Đã phát sinh dữ liệu → chặn xoá
                return Conflict(new { code = "RESTAURANT_HAS_DATA", message = ex.Message });
            }
        }
    }
}
