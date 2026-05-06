using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class Promotion
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public int DiscountType { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public string? Banner { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
