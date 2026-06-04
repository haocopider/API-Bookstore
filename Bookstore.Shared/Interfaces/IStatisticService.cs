using Bookstore.Shared.Dtos;

namespace Bookstore.Shared.Interfaces
{
    public interface IStatisticService
    {
        Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TopSellingBookDto>> GetTopSellingBooksAsync(DateTime startDate, DateTime endDate, int top = 10);

        // Thêm hàm mới cho Dashboard và Chart
        Task<MonthlyRevenueDto> GetDashboardSummaryAsync();
    }
}
