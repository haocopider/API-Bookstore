using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? AuthorId { get; set; }

    public string? Description { get; set; }

    public string? Publisher { get; set; }

    public DateTime? PublishDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public string? CoverImageUrl { get; set; }

    public virtual Author? Author { get; set; }

    public virtual ICollection<BookFormat> BookFormats { get; set; } = new List<BookFormat>();

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
