using Bookstore.Api.Attributes;
using Bookstore.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace Bookstore.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [HasPermission("VIEW_REPORTS")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _statisticService;

        public StatisticsController(IStatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            // Mặc định lấy 30 ngày gần nhất nếu không truyền tham số
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var result = await _statisticService.GetRevenueSummaryAsync(start, end);
            return Ok(result);
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyRevenue([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var result = await _statisticService.GetDailyRevenueAsync(start, end);
            return Ok(result);
        }

        [HttpGet("top-books")]
        public async Task<IActionResult> GetTopBooks([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int top = 10)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var result = await _statisticService.GetTopSellingBooksAsync(start, end, top);
            return Ok(result);
        }
    }
}