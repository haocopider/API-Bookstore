using Bookstore.Shared.Models;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Dtos;
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
                    var updatedBefore = DateTime.UtcNow.AddMinutes(-1);

                    var ordersToUpdate = await dbContext.Orders
                        .Include(o => o.User)
                        .Where(o => o.Status == 1 && o.OrderDate <= updatedBefore)
                        .ToListAsync(stoppingToken);

                    if (ordersToUpdate.Any())
                    {
                        foreach (var order in ordersToUpdate)
                        {
                            try
                            {
                                order.Status = 2;

                                var noti = new Notification
                                {
                                    UserId = order.UserId ?? 0,
                                    Title = "Trạng thái đơn hàng",
                                    Content = $"Đơn hàng {order.OrderCode} đã được đã được giao thành công.",
                                    CreatedAt = DateTime.UtcNow,
                                    IsRead = false,
                                    OrderId = order.Id,
                                };
                                if (order.User != null) noti.User = order.User;
                                dbContext.Notifications.Add(noti);

                                // Gửi push notification thông qua dịch vụ notification (Firebase)
                                var notiService = scope.ServiceProvider.GetService<INotificationService>();
                                if (notiService != null && order.User?.FcmToken != null)
                                {
                                    var sendOk = await notiService.SendNotificationAsync(order.User.FcmToken, noti.Title, noti.Content);
                                    _logger.LogInformation($"Gửi notification cho user {order.UserId} - success={sendOk}");
                                }

                                _logger.LogInformation($"Đã tự động chuyển đơn {order.Id} sang trạng thái 2.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Lỗi khi cập nhật trạng thái đơn {order.Id}");
                            }
                        }
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}
