using System;
using System.Collections.Generic;

namespace Bookstore.Shared.Models;

public partial class Order
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public int? PointsUsed { get; set; }

    public int Status { get; set; }

    public string? ShippingAddress { get; set; }

    public string? PaymentMethod { get; set; }

    public int PaymentStatus { get; set; }

    public string? TrackingNumber { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }
}
