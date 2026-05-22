using Bookstore.Shared.Interfaces;
using static Bookstore.Shared.Dtos.StatisticDto;

namespace Bookstore.Shared.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StatisticService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate)
        {
            // Lấy các đơn hàng hợp lệ (VD: Đã thanh toán / Đã hoàn thành) trong khoảng thời gian
            // Điều chỉnh trạng thái (Status) theo thiết kế DB của bạn
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.OrderDate >= startDate && o.OrderDate <= endDate
            /* && o.Status == 3 */ // Bỏ comment và sửa số 3 thành trạng thái "Hoàn thành" của bạn
            );

            return new RevenueSummaryDto
            {
                TotalRevenue = orders.Sum(o => o.TotalAmount), // Hoặc trường tính tổng tiền tương ứng trong bảng Order
                TotalOrders = orders.Count(),
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.OrderDate >= startDate && o.OrderDate <= endDate
            /* && o.Status == 3 */
            );

            // Nhóm theo ngày và tính tổng
            var dailyStats = orders
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

            return dailyStats;
        }

        public async Task<IEnumerable<TopSellingBookDto>> GetTopSellingBooksAsync(DateTime startDate, DateTime endDate, int top = 10)
        {
            // Lấy tất cả OrderItems của các đơn hàng thành công trong khoảng thời gian
            var orders = await _unitOfWork.Orders.FindAsync(
                o => o.OrderDate >= startDate && o.OrderDate <= endDate /* && o.Status == 3 */,
                o => o.OrderItems // Include chi tiết đơn hàng
            );

            // Do OrderItem có thể liên kết đến Book (hoặc BookFormat), ta cần Join và GroupBy
            // Giả sử OrderItem có trường BookId và Quantity, UnitPrice
            var orderItems = orders.SelectMany(o => o.OrderItems).ToList();

            // Nếu OrderItem không Include được trực tiếp Book, ta lấy danh sách BookId ra trước
            var bookIds = orderItems.Select(oi => oi.ItemId).Distinct().ToList();
            var books = await _unitOfWork.Books.FindAsync(b => bookIds.Contains(b.Id));

            var topBooks = orderItems
                .GroupBy(oi => oi.ItemId)
                .Select(g => new
                {
                    BookId = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.PriceAtPurchase) // Số lượng * Đơn giá
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(top)
                .ToList();

            // Map tên sách vào kết quả
            var result = topBooks.Select(tb => new TopSellingBookDto
            {
                BookId = tb.BookId,
                Title = books.FirstOrDefault(b => b.Id == tb.BookId)?.Title ?? "Sách không xác định",
                TotalQuantitySold = tb.TotalQuantity,
                TotalRevenue = tb.TotalRevenue
            });

            return result;
        }
    }
}