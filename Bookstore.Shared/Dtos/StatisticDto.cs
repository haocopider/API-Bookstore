using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Dtos
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

    public class MonthlyRevenueDto
    {
        public decimal CurrentMonthRevenue { get; set; }
        public int CurrentMonthOrders { get; set; }
        public int TotalActiveProducts { get; set; }
        public int TotalProductsSold { get; set; }
        public int TotalFollowers { get; set; }

        public double RevenueGrowth { get; set; }
        public double OrderGrowth { get; set; }

        public List<MonthlyRevenueStatDto> RevenueChartData { get; set; } = new();
        public List<ProductStatDto> ProductStats { get; set; } = new();
    }

    public class MonthlyRevenueStatDto
    {
        public int Month { get; set; }
        public string MonthLabel => $"T{Month}";
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class YearlyRevenueStatDto
    {
        public int Year { get; set; }
        public string YearLabel => $"Năm {Year}";
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }


    // DTO cho Sách bán chạy
    public class TopSellingBookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProductStatDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int SoldQuantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal Revenue { get; set; }
    }
}
