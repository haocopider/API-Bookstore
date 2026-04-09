using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int? ParentId { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
