using Bookstore.Shared.Dtos;
using Bookstore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bookstore.Shared.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<NotiDto>> GetAllNotificationsAsync(int userId);
        Task<IEnumerable<NotiDto>> GetAdminNotificationsAsync();
        Task<bool> SendNotificationAsync(string fcmToken, string title, string body);
        Task<bool> MaskAsRead(int id);
        Task<bool> RemoveNotification(int id);
    }
}
