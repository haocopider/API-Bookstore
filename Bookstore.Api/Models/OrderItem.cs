using System;
using System.Collections.Generic;

namespace Bookstore.Api.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public string? ItemType { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtPurchase { get; set; }

    public virtual Order? Order { get; set; }
}
