using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int BookId { get; set; }

    public int RatingValue { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ReviewImg { get; set; }

    public virtual Book Book { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
