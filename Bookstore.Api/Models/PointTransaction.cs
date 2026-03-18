using System;
using System.Collections.Generic;

namespace Bookstore.Api.Models;

public partial class PointTransaction
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int Amount { get; set; }

    public string? TransactionType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
