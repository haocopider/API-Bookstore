using System;
using System.Collections.Generic;

namespace Bookstore.Api.Models;

public partial class Accessory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? PointCost { get; set; }

    public int? Stock { get; set; }

    public string? ImageUrl { get; set; }

    public bool? IsDeleted { get; set; }
}
