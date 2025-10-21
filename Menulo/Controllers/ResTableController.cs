using AutoMapper;
using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Application.Features.ResTables.Interfaces;
using Menulo.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
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
        private readonly ILogger<ResTableController> _logger;


        public ResTableController(IMapper mapper, IResTablesService service, ILogger<ResTableController> logger) : base(mapper)
        {
            _service = service;
            _logger = logger;
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
        public async Task<ActionResult<ResTableResponse>> Get(int id, CancellationToken ct)
        {
            var dto = await _service.GetByIdAsync(id, ct);

            return dto is null
                    ? NotFound()
                    : Ok(dto);
        }

        // API 3: Xóa dữ liệu
        [HttpDelete("{id:int}")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        // API 4: Tạo mã QR từ GUID
        // Render QR từ chính TableCode (không phụ thuộc bất cứ URL nào)
        [HttpGet("qr-table/{id:int}")]
        [Produces("image/png")]
        public async Task<IActionResult> GetQrTable(int id, [FromQuery] int scale = 10, CancellationToken ct = default)
        {
            try
            {
                if (id <= 0) return BadRequest("Invalid id.");

                var dto = await _service.GetByIdAsync(id, ct);
                if (dto is null) return NotFound();

                // payload tạm thời: chỉ là TableCode
                // (sau này muốn đổi sang URL/deep-link/JSON thì chỉ thay chuỗi này)
                //var payload = $"{flutterWebAppUrl}?tableCode={Uri.EscapeDataString(dto.TableCode)}";
                var payload = dto.TableCode;
                if (string.IsNullOrWhiteSpace(payload))
                    return Problem("TableCode is missing.", statusCode: 500);

                // giới hạn scale an toàn
                if (scale < 4) scale = 4;
                if (scale > 20) scale = 20;

                using var gen = new QRCodeGenerator();
                using var data = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

                // PNG dạng byte[]
                using var png = new PngByteQRCode(data);
                var bytes = png.GetGraphic(scale);

                // cache/etag để đỡ tốn CPU
                Response.Headers.CacheControl = "public, max-age=86400";
                var etag = $"W/\"{Convert.ToBase64String(System.Security.Cryptography.SHA1.HashData(bytes))}\"";
                Response.Headers.ETag = etag;

                return File(bytes, "image/png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QR render failed for tableId {Id}", id);
                return Problem("Internal error while generating QR.", statusCode: 500);
            }
        }
    }
}
