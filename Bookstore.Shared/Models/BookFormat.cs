using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class BookFormat
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public int FormatType { get; set; }

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public string? Sku { get; set; }

    public string? DigitalLink { get; set; }

    public double? Rating { get; set; }

    public virtual Book Book { get; set; } = null!;
}
