using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class CheckoutItemDto
    {
        public int ItemId { get; set; }
        public string ItemType { get; set; } = "Book";
        public int Quantity { get; set; }
    }

    public class CheckoutResponseDto
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int? OrderId { get; set; }
        public decimal? TotalPaid { get; set; }
    }

    public class CheckoutRequestDto
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;

        public List<CheckoutItemDto> Items { get; set; } = new List<CheckoutItemDto>();
    }
}
