using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menulo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ResTableController : DataTablesControllerBase
    {
        public ResTableController(IMapper mapper) : base(mapper)
        {
        }
    }
}
