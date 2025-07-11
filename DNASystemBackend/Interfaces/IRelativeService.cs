using DNASystemBackend.DTOs;
using DNASystemBackend.Models;

namespace DNASystemBackend.Interfaces
{
    public interface IRelativeService
    {
        Task<IEnumerable<Relative>> GetAllAsync();
        Task<Relative?> GetByIdAsync(string id);
        Task<Relative?> GetByUserIdAsync(string userId);
        Task<Relative> CreateAsync(RelativeCreateDto relative);
        Task<bool> UpdateAsync(string id, Relative updated);
        Task<bool> DeleteAsync(string id);

        Task<Relative?> GetByBookingIdAsync(string bookingId);
    }
}
