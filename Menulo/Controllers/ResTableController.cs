using AutoMapper;
using Menulo.Application.Features.ResTables.Dtos;
using Menulo.Application.Features.ResTables.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            throw new NotImplementedException();
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
