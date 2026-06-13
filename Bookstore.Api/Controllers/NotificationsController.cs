using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notiService;

        public NotificationsController(INotificationService notiService)
        {
            _notiService = notiService;
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId)) return userId;
            throw new UnauthorizedAccessException("Không thể xác thực người dùng.");
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminNotificationsAsync()
        {
            var notifications = await _notiService.GetAdminNotificationsAsync();
            return Ok(notifications);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotifications()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notiService.GetAllNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult<ApiResponse>> MarkAsRead(int id)
        {
            var result = await _notiService.MaskAsRead(id);
            if (!result) return NotFound(new ApiResponse { Success = false, Message = "Không tìm thấy thông báo." });
            return Ok(new ApiResponse { Success = true, Message = "Đánh dấu đã đọc." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveNotification(int id)
        {
            var result = await _notiService.RemoveNotification(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
