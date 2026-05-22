using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. Khách hàng gọi hàm này để lấy/tạo phiên chat hiện tại với cửa hàng
        [HttpGet("my-conversation")]
        public async Task<IActionResult> GetOrCreateMyConversation()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int customerId)) return Unauthorized();

            // Tìm conversation đang mở (Status = 0)
            var convs = await _unitOfWork.Conversations.FindAsync(c => c.CustomerId == customerId && c.Status == 0);
            var activeConv = convs.FirstOrDefault();

            if (activeConv == null)
            {
                activeConv = new Conversation
                {
                    CustomerId = customerId,
                    Status = 0, // 0: Active
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Conversations.AddAsync(activeConv);
                await _unitOfWork.CommitAsync();
            }

            return Ok(new ConversationDto
            {
                Id = activeConv.Id,
                CustomerId = activeConv.CustomerId,
                StaffId = activeConv.StaffId,
                Status = activeConv.Status,
                CreatedAt = activeConv.CreatedAt
            });
        }

        // 2. Tải lịch sử tin nhắn của một đoạn chat
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            // Cần lấy Messages và sắp xếp theo thời gian
            var messages = await _unitOfWork.Messages.FindAsync(m => m.ConversationId == conversationId);

            var history = messages.OrderBy(m => m.CreatedAt).Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                MessageType = m.MessageType,
                CreatedAt = m.CreatedAt
            });

            return Ok(history);
        }

        // 3. Dành cho Admin: Lấy danh sách tất cả các Conversation đang mở
        [HttpGet("admin/active-conversations")]
        // [HasPermission("CUSTOMER_CARE")] // Có thể mở comment khi tích hợp phân quyền
        public async Task<IActionResult> GetActiveConversations()
        {
            // Cần nạp cả thông tin Customer để hiển thị tên
            var convs = await _unitOfWork.Conversations.FindAsync(
                c => c.Status == 0,
                c => c.Customer // Yêu cầu Include Customer
            );

            var result = convs.OrderByDescending(c => c.CreatedAt).Select(c => new ConversationDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer?.FullName,
                StaffId = c.StaffId,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            });

            return Ok(result);
        }
    }
}