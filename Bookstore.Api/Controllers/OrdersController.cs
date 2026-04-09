using Bookstore.Shared.Dtos;
using Bookstore.Shared.DTOs;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Bookstore.Shared.Repositories;
using Bookstore.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("my-orders/{userId}")]
        public async Task<IActionResult> GetMyOrders(int userId)
        {
            var result = await _orderService.GetMyOrdersAsync(userId);
            return Ok(result);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequestDto request)
        {
            // Gọi Business Logic
            var result = await _orderService.CheckoutAsync(request);

            // Xử lý Response
            if (!result.IsSuccess)
            {
                return BadRequest(new { Message = result.ErrorMessage });
            }

            return Ok(new
            {
                Message = "Đặt hàng thành công!",
                OrderId = result.OrderId,
                TotalPaid = result.TotalPaid
            });
        }
    }
}
