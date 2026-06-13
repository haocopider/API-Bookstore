using Bookstore.Api.Attributes;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId)) return userId;
            throw new UnauthorizedAccessException("Không thể xác thực người dùng.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                int userId = GetCurrentUserId();
                var newOrder = await _orderService.CreateOrderAsync(userId, request);

                return Ok(new { message = "Đặt hàng thành công!", order = newOrder });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống khi tạo đơn.", error = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetOrderHistory([FromQuery] int? status)
        {
            try
            {
                int userId = GetCurrentUserId();

                // Nếu status = null, Service sẽ trả về TẤT CẢ đơn hàng.
                // Nếu status = 1, 2, 3, 4, Service sẽ lọc đúng trạng thái đó.
                var orders = await _orderService.GetOrderHistoryAsync(userId, status);

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy lịch sử mua hàng.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                var order = await _orderService.GetOrderDetailAsync(userId, id);

                return Ok(order);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi hệ thống.", error = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<ApiResponse>> CancelOrder(int id, [FromBody] CancelOrderRequest request)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _orderService.CancelOrderAsync(userId, id, request.Reason);

                return Ok(new ApiResponse { Success = true, Message = "Đã hủy đơn hàng thành công." });
            }
            catch (KeyNotFoundException ex) 
            {
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Lỗi hệ thống.", Data = ex.Message });
            }
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult<ApiResponse>> CompleteOrder(int id)
        {
            try
            {
                int userId = GetCurrentUserId();
                await _orderService.CompleteOrderAsync(userId, id);

                return Ok(new ApiResponse { Success = true, Message = "Cảm ơn bạn đã xác nhận nhận hàng!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Lỗi hệ thống.", Data = ex.Message });
            }
        }

        [HttpGet("admin")]
        [HasPermission("RESOUCRES.VIEW")]
        public async Task<IActionResult> GetAllOrdersForAdmin([FromQuery] int? status)
        {
            var orders = await _orderService.GetAllOrdersForAdminAsync(status);
            return Ok(orders);
        }

        [HttpPut("admin/{id}/status")]
        [HasPermission("RESOUCRES.UPDATE")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderStatus(int id, [FromBody] int newStatus)
        {
            // Trạng thái hợp lệ từ 0 đến 4
            if (newStatus < 0 || newStatus > 4)
                return BadRequest(new { message = "Trạng thái không hợp lệ." });

            try
            {
                var success = await _orderService.UpdateOrderStatusByAdminAsync(id, newStatus);

                if (!success)
                    return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy đơn hàng." });

                return Ok(new ApiResponse { Success = true, Message = "Cập nhật trạng thái đơn hàng thành công." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new ApiResponse { Success = false, Message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }

    public class CancelOrderRequest
    {
        public string Reason { get; set; } = "Tôi thay đổi ý định";
    }
}
