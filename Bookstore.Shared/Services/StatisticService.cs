using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Shared.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatisticService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MonthlyRevenueDto> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow; // Hoặc DateTime.Now tùy múi giờ của bạn
            var currentYear = now.Year;
            var currentMonth = now.Month;

            // 1. Xác định thời điểm bắt đầu của truy vấn
            var startOfThisYear = new DateTime(currentYear, 1, 1);
            var lastMonthDate = now.AddMonths(-1);
            var startOfLastMonth = new DateTime(lastMonthDate.Year, lastMonthDate.Month, 1);

            var queryStartDate = startOfLastMonth < startOfThisYear ? startOfLastMonth : startOfThisYear;

            // 2. Lấy tất cả đơn hàng từ mốc thời gian trên đến hiện tại (Kèm OrderItems)
            var recentOrders = await _unitOfWork.Orders.FindAsync(
                o => o.OrderDate >= queryStartDate,
                //&& o.Status == 3, 
                o => o.OrderItems
            );

            // 3. Dữ liệu tháng hiện tại
            var thisMonthOrders = recentOrders.Where(o => o.OrderDate?.Year == currentYear && o.OrderDate?.Month == currentMonth).ToList();
            var currentMonthRevenue = thisMonthOrders.Sum(o => o.TotalAmount);
            var currentMonthOrdersCount = thisMonthOrders.Count;
            var totalProductsSoldThisMonth = thisMonthOrders.SelectMany(o => o.OrderItems).Sum(oi => oi.Quantity);

            // 4. Dữ liệu tháng trước (Để tính % tăng trưởng)
            var lastMonthOrders = recentOrders.Where(o => o.OrderDate?.Year == lastMonthDate.Year && o.OrderDate?.Month == lastMonthDate.Month).ToList();
            var lastMonthRevenue = lastMonthOrders.Sum(o => o.TotalAmount);
            var lastMonthOrdersCount = lastMonthOrders.Count;

            // Tính toán % Tăng trưởng
            double revenueGrowth = lastMonthRevenue == 0
                ? (currentMonthRevenue > 0 ? 100 : 0)
                : (double)((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100;

            double orderGrowth = lastMonthOrdersCount == 0
                ? (currentMonthOrdersCount > 0 ? 100 : 0)
                : (double)(currentMonthOrdersCount - lastMonthOrdersCount) / lastMonthOrdersCount * 100;

            // 5. Dữ liệu biểu đồ 12 tháng (Của năm hiện tại)
            var thisYearOrders = recentOrders.Where(o => o.OrderDate?.Year == currentYear).ToList();
            var chartData = new List<MonthlyRevenueStatDto>();

            for (int i = 1; i <= 12; i++)
            {
                var ordersInMonth = thisYearOrders.Where(o => o.OrderDate?.Month == i).ToList();
                chartData.Add(new MonthlyRevenueStatDto
                {
                    Month = i,
                    Revenue = ordersInMonth.Sum(o => o.TotalAmount),
                    OrderCount = ordersInMonth.Count
                });
            }

            // 6. Lấy dữ liệu cơ bản khác (Sách & Người dùng)
            var activeBooks = await _unitOfWork.Books.FindAsync(b => b.IsDeleted == false);
            var allUsers = await _unitOfWork.Users.GetAllAsync();

            // ----------------------------------------------------------------------
            // 7. [MỚI] TÍNH TOÁN DỮ LIỆU CHO BẢNG THỐNG KÊ SẢN PHẨM
            // ----------------------------------------------------------------------

            // Gom tất cả chi tiết đơn hàng (OrderItems) trong khoảng thời gian đã truy vấn
            var allRecentOrderItems = recentOrders.SelectMany(o => o.OrderItems).ToList();

            var productStats = activeBooks.Select(async book =>
            {
                // Tìm các lượt mua thuộc về cuốn sách này
                var bookSales = allRecentOrderItems.Where(oi => oi.ItemId == book.Id).ToList();
                var bf = await _unitOfWork.BookFormats.GetFirstOrDefaultAsync(b => b.BookId == book.Id);

                return new ProductStatDto
                {
                    ProductName = book.Title,
                    StockQuantity = bf.Stock ?? 0,

                    // Tính tổng số lượng đã bán
                    SoldQuantity = bookSales.Sum(oi => oi.Quantity),

                    // Tính tổng doanh thu: Số lượng * Giá bán tại thời điểm đặt hàng
                    Revenue = bookSales.Sum(oi => oi.Quantity * oi.SnapshotUnitPrice)
                };
            })
                .Where(p => p.Result.SoldQuantity > 0)
            .OrderByDescending(p => p.Result.Revenue)
            .ToList();

            // 8. Trả về kết quả tổng hợp
            return new MonthlyRevenueDto
            {
                CurrentMonthRevenue = currentMonthRevenue,
                CurrentMonthOrders = currentMonthOrdersCount,
                TotalProductsSold = totalProductsSoldThisMonth,
                TotalActiveProducts = activeBooks.Count(),
                TotalFollowers = allUsers.Count(),
                RevenueGrowth = Math.Round(revenueGrowth, 2),
                OrderGrowth = Math.Round(orderGrowth, 2),
                RevenueChartData = chartData,

                ProductStats = productStats.Select(p => p.Result).ToList()
            };
        }
        public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => 
                     o.OrderDate >= startDate && 
                     o.OrderDate <= endDate 
            );

            return new RevenueSummaryDto
            {
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count(),
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.OrderDate >= startDate && o.OrderDate <= endDate
            );

            return orders
                .Where(o => o.OrderDate.HasValue)
                .GroupBy(o => o.OrderDate.Value.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrdersCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();
        }

        public async Task<IEnumerable<TopSellingBookDto>> GetTopSellingBooksAsync(DateTime startDate, DateTime endDate, int top = 10)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.OrderDate >= startDate && o.OrderDate <= endDate,
                o => o.OrderItems
            );

            var orderItems = orders.SelectMany(o => o.OrderItems).ToList();
            var bookIds = orderItems.Select(oi => oi.ItemId).Distinct().ToList();
            var books = await _unitOfWork.Books.FindAsync(b => bookIds.Contains(b.Id));

            var topBooks = orderItems
                .GroupBy(oi => oi.ItemId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.PriceAtPurchase)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(top)
                .ToList();

            return topBooks.Select(tb => new TopSellingBookDto
            {
                BookId = tb.BookId,
                Title = books.FirstOrDefault(b => b.Id == tb.BookId)?.Title ?? "Sách không xác định",
                TotalQuantitySold = tb.TotalQuantity,
                TotalRevenue = tb.TotalRevenue
            });
        }
    }
}