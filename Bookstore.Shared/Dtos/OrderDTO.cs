using System.ComponentModel.DataAnnotations;

namespace Bookstore.Shared.Dtos
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public int PaymentMethod { get; set; } = 0;
        public int PointIsUsed { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderDto
    {
        public int Id { get; set; }
        public string ReceiverName { get; set; }
        public string PhoneNumber { get; set; }
        public string ShippingAddress { get; set; }
        public int PaymentMethod { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int Status { get; set; }
        public int TotalItems => Items.Sum(i => i.Quantity);
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderItemDto
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal OriginalPrice { get; set; }
    }
}
