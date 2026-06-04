using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Bookstore.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                conversationId.ToString()
            );
        }

        public async Task JoinConversation(int conversationId)
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                throw new HubException("Unauthorized");

            var conversation = await _unitOfWork.Conversations
                .GetFirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                throw new HubException("Conversation not found");

            // Lấy chuỗi Role và RoleId trực tiếp từ JWT Token
            var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value
                         ?? Context.User?.FindFirst("Role")?.Value
                         ?? "";

            var roleIdClaim = Context.User?.FindFirst("RoleId")?.Value;

            // Kiểm tra xem User hiện tại có phải là Admin/Staff không
            // (Bao gồm Role = ADMIN, STAFF hoặc RoleId = 3)
            bool isStaff = roleClaim.ToUpper() == "ADMIN"
                        || roleClaim.ToUpper() == "STAFF"
                        || roleIdClaim == "3";

            // Nếu KHÔNG PHẢI là Admin/Staff, VÀ cũng KHÔNG PHẢI chủ phòng chat -> Báo lỗi cấm
            if (!isStaff && conversation.CustomerId != userId)
            {
                throw new HubException("Forbidden");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                conversationId.ToString()
            );
        }

        public async Task SendMessage(int conversationId, string content, string messageType = "Text")
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int senderId))
                throw new HubException("Unauthorized");

            var conversation = await _unitOfWork.Conversations
                .GetFirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
                throw new HubException("Conversation not found");

            // Lấy chuỗi Role và RoleId trực tiếp từ JWT Token giống hàm trên
            var roleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value
                         ?? Context.User?.FindFirst("Role")?.Value
                         ?? "";

            var roleIdClaim = Context.User?.FindFirst("RoleId")?.Value;

            bool isStaff = roleClaim.ToUpper() == "ADMIN"
                        || roleClaim.ToUpper() == "STAFF"
                        || roleIdClaim == "3";

            // Customer chỉ gửi được conversation của mình
            if (!isStaff && conversation.CustomerId != senderId)
            {
                throw new HubException("Forbidden");
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                IsAdmin = isStaff,
                MessageType = messageType,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Messages.AddAsync(message);

            conversation.LastMessageAt = DateTime.UtcNow;

            await _unitOfWork.CommitAsync();


            var response = new
            {
                id = message.Id,
                conversationId = message.ConversationId,
                senderId = message.SenderId,
                isAdmin = isStaff,
                content = message.Content,
                createdAt = message.CreatedAt,
                messageType = message.MessageType
            };

            await Clients
                .Group(conversationId.ToString())
                .SendAsync("ReceiveMessage", response);
        }
    }
}