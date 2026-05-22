using System;
using System.Collections.Generic;
using System.Text;
using static Bookstore.Shared.Dtos.StatisticDto;

namespace Bookstore.Shared.Interfaces
{
    public interface IStatisticService
    {
        // Lấy tổng quan doanh thu trong khoảng thời gian
        Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime startDate, DateTime endDate);

        // Lấy doanh thu theo từng ngày (để vẽ biểu đồ)
        Task<IEnumerable<DailyRevenueDto>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate);

        // Lấy top N sách bán chạy nhất
        Task<IEnumerable<TopSellingBookDto>> GetTopSellingBooksAsync(DateTime startDate, DateTime endDate, int top = 10);
    }
}
