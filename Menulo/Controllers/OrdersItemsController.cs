using Menulo.Application.Common.Interfaces;
using Menulo.Application.Features.Orders.Dtos;
using Menulo.Application.Features.Orders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Menulo.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Orders")]
    public sealed class OrdersItemsController : Controller
    {
        private readonly ICurrentUser _current;
        private readonly IOrdersService _orders;

        public OrdersItemsController(ICurrentUser current, IOrdersService orders)
        {
            _current = current;
            _orders = orders;
        }


        // API 1: POST /api/Orders/{tableId}/items
        /// <summary>
        /// Thêm món vào hóa đơn cho bàn cụ thể. Có thể thêm nhiều món cùng lúc.
        /// Có thể tạo mới hóa đơn nếu chưa có.
        /// </summary>
        [HttpPost("{tableId:int}/items")]
        public async Task<IActionResult> AddOrderItem(int tableId, [FromBody] AddOrderItemsListRequest request, CancellationToken ct)
        {
            if (request == null || request.Items == null || request.Items.Count == 0)
                return BadRequest(new { message = "Không có món để thêm." });

            // user id cho CreatedByUserId ở round
            var userId = _current.UserId;
            try
            {
                await _orders.AddItemsByStaffAsync(tableId, request, userId, ct);
                return Ok(new { message = "Đã thêm món vào hóa đơn thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
