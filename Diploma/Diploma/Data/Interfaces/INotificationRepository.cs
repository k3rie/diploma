using Diploma.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Diploma.Data.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetUnreadByUserIdAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}