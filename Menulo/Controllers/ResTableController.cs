using AutoMapper;
using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Application.Features.ResTables.Interfaces;
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
    public sealed class ResTableController : DataTablesControllerBase
    {
        private readonly IResTablesService _service;


        public ResTableController(IMapper mapper, IResTablesService service) : base(mapper)
        {
            _service = service;
        }


        // API 1: Datatables server-side processing
        [HttpPost("datatable")]
        [ValidateAntiForgeryToken]
        public IActionResult GetDataTable([FromBody] DataTablesRequest request, CancellationToken ct)
        {
            // Lấy IQueryable từ service
            IQueryable<RestaurantTable> source = _service.GetQueryableRestaurantTableForCurrentUser();

            // Xây dựng biểu thức tìm kiếm
            Expression<Func<RestaurantTable, bool>>? searchPredicate = null;
            var searchValue = request.Search?.Value;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var kw = searchValue.Trim().ToLower();
                // Lưu ý: nếu non-superadmin thì cột Restaurant không hiển thị, nhưng search vẫn OK
                searchPredicate = c =>
                    (c.Description != null && c.Description.ToLower().Contains(kw)) ||
                    (c.Restaurant != null && c.Restaurant.Name.ToLower().Contains(kw));
            }

            return GetDataTableResult<RestaurantTable, ResTableResponse>(source, request, searchPredicate);
        }

        // API 2: Lấy dữ liệu để hiển thị cho view chi tiết và xóa
        [HttpGet("{id:int}")]
        public Task<ActionResult<ResTableResponse>> Get(int id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        // API 3: Xóa dữ liệu
        [HttpDelete("{id:int}")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
