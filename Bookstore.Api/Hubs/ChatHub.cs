using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Bookstore.Api.Hubs
{
    [Authorize] // Bắt buộc phải có JWT Token (Khách hoặc Nhân viên)
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Gọi khi người dùng (Khách hoặc Admin) bấm vào xem một cuộc hội thoại
        public async Task JoinConversation(int conversationId)
        {
            // Thêm Connection hiện tại vào Group mang tên ID của cuộc hội thoại
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        // Rời khỏi cuộc hội thoại (khi tắt khung chat)
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        // Gửi tin nhắn
        public async Task SendMessage(int conversationId, string content, string messageType = "Text")
        {
            // 1. Lấy ID của người gửi từ JWT Token
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int senderId))
                throw new HubException("Không xác định được danh tính người gửi.");

            // 2. Lưu tin nhắn vào Database
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                MessageType = messageType,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.CommitAsync();

            // 3. Chuẩn bị payload trả về cho Client
            var messageResponse = new
            {
                id = message.Id,
                conversationId = message.ConversationId,
                senderId = message.SenderId,
                content = message.Content,
                messageType = message.MessageType,
                createdAt = message.CreatedAt
            };

            // 4. Phát tin nhắn đến TẤT CẢ những ai đang ở trong Group (kể cả người vừa gửi)
            // Client sẽ lắng nghe sự kiện "ReceiveMessage" để update UI
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", messageResponse);
        }
    }
}