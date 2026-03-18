using System;
using System.Collections.Generic;

namespace Bookstore.Api.Models;

public partial class Conversation
{
    public int Id { get; set; }

    public int? CustomerId { get; set; }

    public int? StaffId { get; set; }

    public int? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? Customer { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
