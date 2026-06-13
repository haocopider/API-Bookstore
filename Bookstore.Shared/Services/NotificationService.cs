using AutoMapper;
using Bookstore.Shared.Dtos;
using Bookstore.Shared.Interfaces;
using Bookstore.Shared.Models;

namespace Bookstore.Shared.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<NotiDto>> GetAdminNotificationsAsync()
        {
            var orders = await _unitOfWork.Orders.FindAsync(o => o.Status == 0);
            var notis = new List<NotiDto>();
            foreach (var order in orders)
            {
                var noti = new NotiDto()
                {
                    Title = $"Đơn hàng mới",
                    Content = $"Vui lòng xác nhận đơn hàng mới với mã {order.OrderCode}.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    OrderId = order.Id
                };
                notis.Add(noti);
            }
            return notis;
        }

        public async Task<IEnumerable<NotiDto>> GetAllNotificationsAsync(int userId)
        {
            var notis = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId, n => n.Order);
            return _mapper.Map<IEnumerable<NotiDto>>(notis);
        }

        public async Task<bool> MaskAsRead(int id)
        {
            var noti = await _unitOfWork.Notifications.GetFirstOrDefaultAsync(n => n.Id == id);
            if (noti == null) return false;

            noti.IsRead = true;
            _unitOfWork.Notifications.Update(noti);
            return true;
        }

        public async Task<bool> RemoveNotification(int id)
        {
            var noti = await _unitOfWork.Notifications.GetFirstOrDefaultAsync(n => n.Id == id);
            if (noti == null) return false;

            _unitOfWork.Notifications.Remove(noti);
            return true;
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body)
        {
            if (string.IsNullOrEmpty(fcmToken)) return false;

            var message = new FirebaseAdmin.Messaging.Message()
            {
                Token = fcmToken,
                Notification = new FirebaseAdmin.Messaging.Notification()
                {
                    Title = title,
                    Body = body
                },
                // Có thể đính kèm Data để Flutter click vào thông báo mở đúng trang đơn hàng
                Data = new Dictionary<string, string>()
                {
                    { "type", "order_update" }
                }
            };

            try
            {
                string response = await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== FIREBASE ERROR =====");
                Console.WriteLine($"Message: {ex.Message}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return false;
            }
        }
    }
}
