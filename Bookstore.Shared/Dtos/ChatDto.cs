using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int? ConversationId { get; set; }
        public int SenderId { get; set; } // Dùng chung cho cả CustomerId và StaffId
        public string Content { get; set; } = null!;
        public string? MessageType { get; set; } // "Text", "Image", etc.
        public DateTime? CreatedAt { get; set; }
    }

    public class ConversationDto
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? StaffId { get; set; }
        public int? Status { get; set; } // 0: Đang mở, 1: Đã đóng
        public DateTime? CreatedAt { get; set; }
        public MessageDto? LastMessage { get; set; }
    }
}
