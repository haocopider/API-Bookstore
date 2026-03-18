using System;
using System.Collections.Generic;

namespace Bookstore.Api.Models;

public partial class Rank
{
    public int Id { get; set; }

    public string RankName { get; set; } = null!;

    public decimal MinSpending { get; set; }

    public double? DiscountRate { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
