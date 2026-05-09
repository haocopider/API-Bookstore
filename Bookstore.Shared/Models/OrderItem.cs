using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public string? ItemType { get; set; }

    public int ItemId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtPurchase { get; set; }

    public string SnapshotBookTitle { get; set; } = null!;

    public decimal SnapshotUnitPrice { get; set; }

    public string? SnapshotBookImg { get; set; }

    public virtual Book Item { get; set; } = null!;

    public virtual Order? Order { get; set; }
}
