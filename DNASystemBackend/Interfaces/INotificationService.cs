using DNASystemBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNASystemBackend.Interfaces
{
    public interface INotificationService
    {
        Task<Notification> GetNotificationAsync(string id);
        Task<IEnumerable<Notification>> GetNotificationsAsync(string userId);
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task UpdateNotificationAsync(Notification notification);
        Task DeleteNotificationAsync(string id);
    }
}