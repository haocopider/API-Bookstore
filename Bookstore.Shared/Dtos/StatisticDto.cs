using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
{
    public class StatisticDto
    {
        public class RevenueSummaryDto
        {
            public decimal TotalRevenue { get; set; }
            public int TotalOrders { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        // DTO cho biểu đồ doanh thu theo từng ngày
        public class DailyRevenueDto
        {
            public DateTime Date { get; set; }
            public decimal Revenue { get; set; }
            public int OrdersCount { get; set; }
        }

        // DTO cho Sách bán chạy
        public class TopSellingBookDto
        {
            public int BookId { get; set; }
            public string Title { get; set; } = string.Empty;
            public int TotalQuantitySold { get; set; }
            public decimal TotalRevenue { get; set; }
        }
    }
}
