using Bookstore.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookstore.Shared.Services
{
    public class OrderStatusUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OrderStatusUpdaterService> _logger;

        public OrderStatusUpdaterService(IServiceProvider serviceProvider, ILogger<OrderStatusUpdaterService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Dịch vụ tự động cập nhật đơn hàng đang chạy...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

                    var ordersToUpdate = await dbContext.Orders
                        .Where(o => o.Status == 0 && o.OrderDate <= oneMinuteAgo)
                        .ToListAsync(stoppingToken);

                    if (ordersToUpdate.Any())
                    {
                        foreach (var order in ordersToUpdate)
                        {
                            order.Status = 1;
                            _logger.LogInformation($"Đã tự động chuyển đơn {order.Id} sang Đang giao.");
                        }
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
