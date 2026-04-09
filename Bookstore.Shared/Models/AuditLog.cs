using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class AuditLog
{
    public int Id { get; set; }

    public int? AdminId { get; set; }

    public string ActionType { get; set; } = null!;

    public string TableName { get; set; } = null!;

    public string? RecordId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Admin? Admin { get; set; }
}
