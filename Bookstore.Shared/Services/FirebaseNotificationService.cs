using FirebaseAdmin.Messaging;
using System.Threading.Tasks;

namespace Bookstore.Shared.Services
{
    public class FirebaseNotificationService
    {
        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body)
        {
            if (string.IsNullOrEmpty(fcmToken)) return false;

            var message = new Message()
            {
                Token = fcmToken,
                Notification = new Notification()
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
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
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