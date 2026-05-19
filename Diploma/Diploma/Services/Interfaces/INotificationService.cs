using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diploma.Services.Interfaces
{
    public interface INotificationService
    {
        Task NotifyAsync(int userId, string title, string message);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
    }

    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public System.DateTime CreatedAt { get; set; }
    }
}