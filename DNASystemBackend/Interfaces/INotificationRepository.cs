using DNASystemBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DNASystemBackend.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> GetByIdAsync(string id);
        Task<IEnumerable<Notification>> GetAllAsync(string userId);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAsync(string id);
    }
}