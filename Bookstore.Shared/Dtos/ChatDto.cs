using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int? ConversationId { get; set; }
        public int SenderId { get; set; }
        public bool IsAdmin { get; set; }
        public string Content { get; set; } = null!;
        public string? MessageType { get; set; } // "Text", "Image", etc.
        public DateTime? CreatedAt { get; set; }
    }

    public class MessageResponse
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public DateTime CreatedAt { get; set; }
    }

    public class ConversationDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? Status { get; set; } 
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public MessageDto? LastMessage { get; set; }
    }
}
