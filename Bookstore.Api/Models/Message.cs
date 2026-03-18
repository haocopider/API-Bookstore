using System;
using System.Collections.Generic;

namespace Bookstore.Api.Models;

public partial class Message
{
    public int Id { get; set; }

    public int? ConversationId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; } = null!;

    public string? MessageType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Conversation? Conversation { get; set; }
}
