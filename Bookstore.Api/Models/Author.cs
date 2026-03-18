using System;
using System.Collections.Generic;

namespace Bookstore.Api.Models;

public partial class Author
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
