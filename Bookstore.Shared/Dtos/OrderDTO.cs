using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.DTOs
{
    public class OrderItemDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
        public string SnapshotBookTitle { get; set; } = null!; // Lấy tên sách lúc mua
        public string SnapshotFormatName { get; set; } = null!;
        public decimal SnapshotUnitPrice { get; set; } // Giá lúc mua
    }

    public class OrderHistoryDto
    {
        public int Id { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal FinalAmount { get; set; }
        public int Status { get; set; } // 0: Chờ duyệt, 1: Đang giao, 2: Hoàn thành...
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }
}
