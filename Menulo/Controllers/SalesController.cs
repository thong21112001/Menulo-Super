using AutoMapper;
using Menulo.Application.Features.Sales.Dtos;
using Menulo.Application.Features.Sales.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using static Menulo.Application.Common.Contracts.DataTables.DataTablesModels;

namespace Menulo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesController : DataTablesControllerBase
    {
        private readonly ISaleService _service;

        public SalesController(IMapper mapper, ISaleService service) : base(mapper)
        {
            _service = service;
        }


        // API 1: Datatables server-side processing
        [HttpPost("datatable")]
        [ValidateAntiForgeryToken]
        public IActionResult GetDataTable([FromBody] DataTablesRequest request, CancellationToken ct)
        {
            // Lấy IQueryable từ service
            IQueryable<SaleRowDto> source = _service.GetQueryableSales();

            // Xây dựng biểu thức tìm kiếm
            Expression<Func<SaleRowDto, bool>>? searchPredicate = null;
            var searchValue = request.Search?.Value;
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var kw = searchValue.Trim().ToLower();
                searchPredicate = s =>
                       s.FullName.ToLower().Contains(kw) ||
                       s.Username.ToLower().Contains(kw) ||
                       s.Email.ToLower().Contains(kw) ||
                       (s.PhoneNumber != null && s.PhoneNumber.Contains(kw));
            }

            //(Nguồn và Đích đều là SaleRowDto)
            return GetDataTableResult<SaleRowDto, SaleRowDto>(source, request, searchPredicate);
        }

        // API 2: Lấy dữ liệu để hiển thị cho view chi tiết và xóa
        [HttpGet("{id}")]
        public async Task<ActionResult<SaleDto>> Get(string id, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(id, ct);

            return dto is null
                    ? NotFound()
                    : Ok(dto);
        }

        // API 3: Xóa dữ liệu
        [HttpDelete("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, CancellationToken ct)
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
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
